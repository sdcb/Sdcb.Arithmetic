<Query Kind="Statements">
  <NuGetReference>Microsoft.CodeAnalysis.CSharp</NuGetReference>
  <NuGetReference>Microsoft.CodeAnalysis.CSharp.Workspaces</NuGetReference>
  <NuGetReference>OpenAI</NuGetReference>
  <Namespace>Microsoft.CodeAnalysis</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp</Namespace>
  <Namespace>Microsoft.CodeAnalysis.CSharp.Syntax</Namespace>
  <Namespace>Microsoft.CodeAnalysis.Text</Namespace>
  <Namespace>OpenAI_API</Namespace>
  <Namespace>OpenAI_API.Chat</Namespace>
  <Namespace>Microsoft.CodeAnalysis.Formatting</Namespace>
</Query>

#nullable enable

string solutionRoot = GetParentDirectoryUntilContainsFile(new DirectoryInfo(Util.CurrentQueryPath), "Sdcb.Arithmetic.sln").ToString();
string file = Path.Combine(solutionRoot, @"Sdcb.Arithmetic.Gmp/GmpFloat.cs");
string code = File.ReadAllText(file);

SyntaxTree tree = CSharpSyntaxTree.ParseText(code, new CSharpParseOptions().WithDocumentationMode(DocumentationMode.Parse));
CompilationUnitSyntax root = tree.GetCompilationUnitRoot();

ClassDeclarationSyntax theClass = root.DescendantNodes()
	.OfType<ClassDeclarationSyntax>()
	.Single(x => x.Identifier.ToString() == "GmpFloat");

MethodReplacer.ReplaceMethods(theClass).ToFullString().Dump();


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

class CommentRemover : CSharpSyntaxRewriter
{
	public override SyntaxTrivia VisitTrivia(SyntaxTrivia trivia)
	{
		// If the trivia is a comment, we remove it by not returning it.
		if (trivia.IsKind(SyntaxKind.SingleLineCommentTrivia) ||
			trivia.IsKind(SyntaxKind.MultiLineCommentTrivia) ||
			trivia.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia) ||
			trivia.IsKind(SyntaxKind.MultiLineDocumentationCommentTrivia))
		{
			return default;
		}

		// Otherwise, we visit the trivia normally.
		return base.VisitTrivia(trivia);
	}

	public static ClassDeclarationSyntax RemoveComments(ClassDeclarationSyntax classDeclaration)
	{
		var commentRemover = new CommentRemover();
		return (ClassDeclarationSyntax)commentRemover.Visit(classDeclaration);
	}
}

class MethodReplacer : CSharpSyntaxRewriter
{
	private string _dummyClassCode;
	private int totalCount;
	private int currentCount;

	public MethodReplacer(ClassDeclarationSyntax classSyntax)
	{
		ClassDeclarationSyntax dummyClass = (ClassDeclarationSyntax)Formatter.Format(
			CommentRemover.RemoveComments(
			MethodRemover.RemoveMethodsAndOperators(classSyntax)), new AdhocWorkspace());
		_dummyClassCode = dummyClass.ToFullString();
		totalCount = classSyntax.ChildNodes().OfType<MethodDeclarationSyntax>().Count();
	}

	public override SyntaxNode? VisitMethodDeclaration(MethodDeclarationSyntax node)
	{
		Console.WriteLine($"Executing {++currentCount} of {totalCount}, name={node.Identifier.ToString()}...");
		
		// Get the full text of the method declaration, including XML comments.
		string comment = node.GetLeadingTrivia().ToFullString();
		string body = node.ToString();

		// Get the new method code from GetReplacementForFunction.
		string newComment = GetReplacementForFunction(comment, body);

		// Parse the new method code into a MethodDeclarationSyntax.
		MethodDeclarationSyntax? newMethod = SyntaxFactory.ParseMemberDeclaration(newComment + "\n" + body) as MethodDeclarationSyntax;

		// Replace the old method with the new one.
		return newMethod;
	}

	private string GetReplacementForFunction(string comment, string functionCode)
	{		
		OpenAIAPI api = OpenAIAPI.ForAzure(Util.GetPassword("azure-ai-resource"), Util.GetPassword("azure-ai-model"), Util.GetPassword("azure-ai-key"));
		api.ApiVersion = "2023-03-15-preview";
		Conversation chat = api.Chat.CreateConversation();

		chat.AppendSystemMessage($"You're a helper C# code optimization robot, please check user prompt carefully and response code only, nothing else.");
		chat.AppendSystemMessage($"""
			User will provide some functions, you need to add/optimize xml comment to the function, and returns xml comment only, function body no need returned, Here is some C# code context to help you understand:
			{_dummyClassCode}
""");

		chat.AppendUserInput("""
			/// <summary>TODO</summary>
			public unsafe static GmpFloat From(int val)
			{
			    Mpf_t raw = new();
			    Mpf_t* ptr = &raw;
			    GmpLib.__gmpf_init_set_si((IntPtr)ptr, val);
			    return new GmpFloat(raw);
			}
""");
		chat.AppendExampleChatbotOutput("""
		    /// <summary>
		    /// Create a <see cref="GmpFloat"/> instance from a integer <paramref name="val"/>, precision default to <see cref="DefaultPrecision"/> in bit.
		    /// </summary>
		    /// <param name="val">The integer value to convert.</param>
		    /// <returns>A new instance of <see cref="GmpFloat"/> representing the converted value.</returns>
""");
		chat.AppendUserInput(functionCode);
		string result = chat.GetResponseFromChatbotAsync().GetAwaiter().GetResult();
		return result;
	}

	public static ClassDeclarationSyntax ReplaceMethods(ClassDeclarationSyntax classDeclaration)
	{
		var rewriter = new MethodReplacer(classDeclaration);
		return (ClassDeclarationSyntax)rewriter.Visit(classDeclaration);
	}
}