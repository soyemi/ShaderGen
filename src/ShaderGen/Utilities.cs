﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace ShaderGen
{
    internal static class Utilities
    {
        public static string GetFullTypeName(this SemanticModel model, TypeSyntax type)
        {
            if (model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }
            if (type.SyntaxTree != model.SyntaxTree)
            {
                model = GetSemanticModel(model.Compilation, type.SyntaxTree);
            }


            TypeInfo typeInfo = model.GetTypeInfo(type);
            if (typeInfo.Type == null)
            {
                typeInfo = model.GetSpeculativeTypeInfo(0, type, SpeculativeBindingOption.BindAsTypeOrNamespace);
                if (typeInfo.Type == null || typeInfo.Type is IErrorTypeSymbol)
                {
                    throw new InvalidOperationException("Unable to resolve type: " + type + " at " + type.GetLocation());
                }
                if (typeInfo.Type != null)
                {
                    return type.ToFullString();
                }
            }

            string ns = GetFullNamespace(typeInfo.Type.ContainingNamespace);
            return ns + "." + typeInfo.Type.Name;
        }

        private static SemanticModel GetSemanticModel(Compilation compilation, SyntaxTree syntaxTree)
        {
            return compilation.GetSemanticModel(syntaxTree);
        }

        public static string GetFullNamespace(INamespaceSymbol ns)
        {
            Debug.Assert(ns != null);
            string currentNamespace = ns.Name;
            if (ns.ContainingNamespace != null && !ns.ContainingNamespace.IsGlobalNamespace)
            {
                return GetFullNamespace(ns.ContainingNamespace) + "." + currentNamespace;
            }
            else
            {
                return currentNamespace;
            }
        }

        public static string GetFullNamespace(SyntaxNode node)
        {
            if (!SyntaxNodeHelper.TryGetParentSyntax(node, out NamespaceDeclarationSyntax namespaceDeclarationSyntax))
            {
                return string.Empty; // or whatever you want to do in this scenario
            }

            string namespaceName = namespaceDeclarationSyntax.Name.ToString();
            return namespaceName;
        }

        private static readonly HashSet<string> s_basicNumericTypes = new HashSet<string>()
        {
            "System.Numerics.Vector2",
            "System.Numerics.Vector3",
            "System.Numerics.Vector4",
            "System.Numerics.Matrix4x4",
        };

        public static bool IsBasicNumericType(string fullName)
        {
            return s_basicNumericTypes.Contains(fullName);
        }
    }
}