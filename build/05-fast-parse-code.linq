<Query Kind="Statements">
  <NuGetReference>Microsoft.CodeAnalysis.CSharp</NuGetReference>
  <NuGetReference>Microsoft.CodeAnalysis.CSharp.Workspaces</NuGetReference>
  <NuGetReference>Microsoft.EntityFrameworkCore.Sqlite</NuGetReference>
  <NuGetReference>OpenAI</NuGetReference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
  <Namespace>Microsoft.CodeAnalysis.Formatting</Namespace>
  <Namespace>Microsoft.CodeAnalysis.Text</Namespace>
  <Namespace>OpenAI_API</Namespace>
  <Namespace>OpenAI_API.Chat</Namespace>
  <Namespace>Microsoft.EntityFrameworkCore</Namespace>
  <Namespace>System.Security.Cryptography</Namespace>
  <Namespace>System.ComponentModel.DataAnnotations</Namespace>
  <Namespace>System.Threading.Tasks</Namespace>
</Query>

#nullable enable

string solutionRoot = GetParentDirectoryUntilContainsFile(new DirectoryInfo(Util.CurrentQueryPath), "Sdcb.Arithmetic.sln").ToString();
string file = Path.Combine(solutionRoot, @"Sdcb.Arithmetic.Mpfr/MpfrFloat.cs");
string code = File.ReadAllText(file);

SyntaxTree tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions().WithDocumentationMode(DocumentationMode.Parse));
CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

ClassDeclarationSyntax theClass = root.DescendantNodes()
	.OfType<ClassDeclarationSyntax>()
	.Single(x => x.Identifier.ToString() == "MpfrFloat");

(await MethodReplacer.ReplaceMethods(theClass, QueryCancelToken)).ToFullString().Dump();


static DirectoryInfo GetParentDirectoryUntilContainsFile(DirectoryInfo? directory, string fileName)
{
	while (directory != null && !File.Exists(Path.Combine(directory.FullName, fileName)))
	{
		directory = directory.Parent;
	}

	if (directory == null)
	{
		throw new InvalidOperationException($"Could not find parent directory containing file '{fileName}'.");
	}

	return directory;
}

class MethodRemover : CSharpSyntaxRewriter
{
	public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		// By not calling the base implementation and returning null, we remove the node from the tree.
		return null;
	}

	public override SyntaxNode? VisitOperatorDeclaration(OperatorDeclarationSyntax node)
	{
		// By not calling the base implementation and returning null, we remove the node from the tree.
		return null;
	}

	public override SyntaxNode? VisitPropertyDeclaration(PropertyDeclarationSyntax node)
	{
		return null;
	}

	public override SyntaxNode? VisitConversionOperatorDeclaration(ConversionOperatorDeclarationSyntax node)
	{
		// By not calling the base implementation and returning null, we remove the node from the tree.
		return null;
	}

	public static ClassDeclarationSyntax RemoveMethodsAndOperators(ClassDeclarationSyntax classDeclaration)
	{
		var rewriter = new MethodRemover();
		return (ClassDeclarationSyntax)rewriter.Visit(classDeclaration);
	}
}

class MethodReplacer : CSharpSyntaxRewriter
{
	public MethodReplacer()
	{
	}

	public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node) => TransformNode(node);

	SyntaxNode TransformNode(MethodDeclarationSyntax node)
	{
		if (node.ReturnType.ToString() == "int" && node.ParameterList.Parameters.Any(x => x.Type.ToString() == "MpfrRounding?"))
		{
			// Get the full text of the method declaration, including XML comments.
			string comment = node.GetLeadingTrivia().ToFullString();
			string body = node.ToString();

			// Get the new method code from GetReplacementForFunction.
			string newComment = GetReplacementForFunction(comment, body);
			return SyntaxFactory.ParseMemberDeclaration("\n" + newComment + "\n" + body + "\n")!;
		}
		return node;
	}

	private string GetReplacementForFunction(string comment, string body)
	{
		Match m = Regex.Match(comment, @"<returns>(.+)</returns>", RegexOptions.Singleline);
		if (!m.Success) return comment;
		
		return comment.Replace(m.Groups[0].Value, 
			"<returns>Returns a ternary value indicating the success of the operation. If the value is zero, the result stored in the destination variable is exact. If the value is positive, the result is greater than the exact result, and if the value is negative, the result is lower than the exact result. If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. A NaN result always corresponds to an exact return value. The opposite of the returned ternary value is representable in an int.</returns>");
	}

	public static async Task<ClassDeclarationSyntax> ReplaceMethods(ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken = default)
	{
		var rewritter = new MethodReplacer();
		return (ClassDeclarationSyntax)rewritter.Visit(classDeclaration);
	}
}