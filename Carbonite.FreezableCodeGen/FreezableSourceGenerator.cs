﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Carbonite.FreezableCodeGen
{
    [Generator]
    public class FreezableSourceGenerator : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxReceiver is FreezableSyntaxReceiver receiver)
            {
                foreach (StructDeclarationSyntax structDeclaration in receiver.FreezableStructs)
                {
                    this.EmitFreezableImplementation(ref context, structDeclaration.Keyword, structDeclaration.Identifier, structDeclaration.Members, structDeclaration.Parent, structDeclaration.SyntaxTree);
                }

                foreach (ClassDeclarationSyntax classDeclaration in receiver.FreezableClasses)
                {
                    this.EmitFreezableImplementation(ref context, classDeclaration.Keyword, classDeclaration.Identifier, classDeclaration.Members, classDeclaration.Parent, classDeclaration.SyntaxTree);
                }
            }

        }

        private void EmitFreezableImplementation(ref GeneratorExecutionContext context, SyntaxToken declarationKeyword, SyntaxToken identifier, IEnumerable<MemberDeclarationSyntax> members, SyntaxNode declarationParent, SyntaxTree tree)
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine("// <auto-generated/>");
            builder.AppendLine($"// Generated IFreezable<TSelf> implementation for struct {identifier}.");
            builder.AppendLine();

            builder.AppendLine("using Carbonite;");
            builder.AppendLine();

            // Go through the containing nodes, writing out namespaces and containing types as we find them
            SyntaxNode node = declarationParent;
            SemanticModel model = context.Compilation.GetSemanticModel(tree, false);
            Stack<string> containingDeclarations = new Stack<string>();
            while (node != null)
            {
                if (node is NamespaceDeclarationSyntax namespaceDeclaration)
                {
                    containingDeclarations.Push($"namespace {namespaceDeclaration.Name}");
                }
                else if (node is ClassDeclarationSyntax classDeclaration)
                {
                    containingDeclarations.Push($"{classDeclaration.Modifiers} class {classDeclaration.Identifier}");
                }
                else if (node is StructDeclarationSyntax structDeclaration)
                {
                    containingDeclarations.Push($"{structDeclaration.Modifiers} struct {structDeclaration.Identifier}");
                }

                node = node.Parent;
            }
            int containedDepth = containingDeclarations.Count;
            string indent = "";
            while (containingDeclarations.Count > 0)
            {
                builder.Append(indent);
                builder.AppendLine(containingDeclarations.Pop());
                builder.Append(indent);
                builder.AppendLine("{");

                indent += "    ";
            }

            builder.AppendLine($"{indent}public partial {declarationKeyword} {identifier} : IFreezable<{identifier}>");
            builder.AppendLine($"{indent}{{");

            builder.AppendLine($"{indent}    public static void Freeze(ICarboniteImagePageWriter pageWriter, int pageOffset, in {identifier} value)");
            builder.AppendLine($"{indent}    {{");

            // Go through the properties
            string sizeExpression = "0";
            foreach (MemberDeclarationSyntax member in members)
            {
                if (member is FieldDeclarationSyntax fieldDeclaration)
                {
                    bool isIn = false;
                    string propertySize = "";
                    string typeArgument = "";
                    if (fieldDeclaration.Declaration.Type is PredefinedTypeSyntax predefinedType)
                    {
                        isIn = false;
                        if (predefinedType.Keyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.StringKeyword))
                        {
                            propertySize = "CarboniteImageWriter.StringSize";
                        }
                        else
                        {
                            propertySize = $"sizeof({predefinedType.Keyword})";
                        }
                    }
                    else if (fieldDeclaration.Declaration.Type is NameSyntax typeName)
                    {
                        // Determine whether this is a value type (struct) or reference type (class)
                        if (model.GetTypeInfo(typeName).Type.IsValueType)
                        {
                            isIn = true;
                            propertySize = $"{typeName}.FrozenSize";
                            typeArgument = $"<{typeName}>";
                        }
                        else
                        {
                            isIn = false;
                            propertySize = "CarboniteImageWriter.PointerSize";
                            typeArgument = $"<{typeName}>";
                        }
                    }
                    else if (fieldDeclaration.Declaration.Type is ArrayTypeSyntax arrayType)
                    {
                        isIn = false;
                        propertySize = "CarboniteImageWriter.ArraySize";
                        if (arrayType.ElementType is PredefinedTypeSyntax predefinedElementType)
                        {
                            typeArgument = "";
                        }
                        else
                        {
                            typeArgument = $"<{arrayType.ElementType}>";
                        }
                    }
                    else
                    {
                        //context.ReportDiagnostic(Diagnostic.Create(new DiagnosticDescriptor("FIOG001", $"Field {member} in {identifier} is of unsupported type {fieldDeclaration.Declaration.Type}.", "", "FrozenImageIO.Generator", DiagnosticSeverity.Warning, true), Location.Create(structDeclaration.SyntaxTree, fieldDeclaration.Declaration.Type.Span)));
                        continue;
                    }
                    builder.AppendLine($"{indent}        pageWriter.Write{typeArgument}(pageOffset + {sizeExpression}, {(isIn ? "in " : "")}value.{fieldDeclaration.Declaration.Variables[0]});");
                    sizeExpression += $" + {propertySize}";
                }
            }


            builder.AppendLine($"{indent}    }}");

            builder.AppendLine($"{indent}    public static int FrozenSize => {sizeExpression};");

            builder.AppendLine($"{indent}    public static bool IsReferenceType => {declarationKeyword.IsKind(Microsoft.CodeAnalysis.CSharp.SyntaxKind.ClassKeyword).ToString().ToLowerInvariant()};");

            builder.AppendLine($"{indent}}}");

            for (int i = 0; i < containedDepth; i++)
            {
                indent = indent.Substring(0, indent.Length - 4);
                builder.AppendLine($"{indent}}}");
            }

            context.AddSource($"{identifier}.g.cs", builder.ToString());
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new FreezableSyntaxReceiver());
        }

        internal class FreezableSyntaxReceiver : ISyntaxReceiver
        {
            private List<StructDeclarationSyntax> _freezableStructs { get; } = new List<StructDeclarationSyntax>();
            public IReadOnlyList<StructDeclarationSyntax> FreezableStructs => this._freezableStructs.AsReadOnly();

            private List<ClassDeclarationSyntax> _freezableClasses { get; } = new List<ClassDeclarationSyntax>();
            public IReadOnlyList<ClassDeclarationSyntax> FreezableClasses => this._freezableClasses.AsReadOnly();

            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                if (syntaxNode is StructDeclarationSyntax structDeclaration)
                {
                    foreach (AttributeListSyntax attributeList in structDeclaration.AttributeLists)
                    {
                        foreach (AttributeSyntax attribute in attributeList.Attributes)
                        {
                            if (attribute.Name is IdentifierNameSyntax attributeName && attributeName.Identifier.ValueText == "GenerateFreezable")
                            {
                                this._freezableStructs.Add(structDeclaration);
                                return;
                            }
                        }
                    }
                }
                else if (syntaxNode is ClassDeclarationSyntax classDeclaration)
                {
                    foreach (AttributeListSyntax attributeList in classDeclaration.AttributeLists)
                    {
                        foreach (AttributeSyntax attribute in attributeList.Attributes)
                        {
                            if (attribute.Name is IdentifierNameSyntax attributeName && attributeName.Identifier.ValueText == "GenerateFreezable")
                            {
                                this._freezableClasses.Add(classDeclaration);
                                return;
                            }
                        }
                    }
                }
            }
        }
    }
}
