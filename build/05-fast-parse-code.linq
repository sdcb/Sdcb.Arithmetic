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
			return SyntaxFactory.ParseMemberDeclaration(newComment.TrimEnd() + "\n" + body + "\n")!;
		}
		return node;
	}

	static HashSet<string> knownSpecialApis = new HashSet<string>
	{
		"mpfr_set","mpfr_set_ui","mpfr_set_si","mpfr_set_uj","mpfr_set_sj","mpfr_set_flt","mpfr_set_d","mpfr_set_ld",
		"mpfr_set_float128","mpfr_set_decimal64","mpfr_set_decimal128","mpfr_set_z","mpfr_set_q","mpfr_set_f","mpfr_set_ui_2exp","mpfr_set_si_2exp",
		"mpfr_set_uj_2exp","mpfr_set_sj_2exp","mpfr_set_z_2exp","mpfr_set_str","mpfr_strtofr","mpfr_init_set_str","mpfr_frexp","mpfr_get_z",
		"mpfr_get_f","mpfr_fits_ulong_p","mpfr_fits_slong_p","mpfr_fits_uint_p","mpfr_fits_sint_p","mpfr_fits_ushort_p","mpfr_fits_sshort_p","mpfr_fits_uintmax_p",
		"mpfr_fits_intmax_p","mpfr_add","mpfr_add_ui","mpfr_add_si","mpfr_add_d","mpfr_add_z","mpfr_add_q","mpfr_sub",
		"mpfr_ui_sub","mpfr_sub_ui","mpfr_si_sub","mpfr_sub_si","mpfr_d_sub","mpfr_sub_d","mpfr_z_sub","mpfr_sub_z",
		"mpfr_sub_q","mpfr_mul","mpfr_mul_ui","mpfr_mul_si","mpfr_mul_d","mpfr_mul_z","mpfr_mul_q","mpfr_sqr",
		"mpfr_div","mpfr_ui_div","mpfr_div_ui","mpfr_si_div","mpfr_div_si","mpfr_d_div","mpfr_div_d","mpfr_div_z",
		"mpfr_div_q","mpfr_sqrt","mpfr_sqrt_ui","mpfr_rec_sqrt","mpfr_cbrt","mpfr_rootn_ui","mpfr_rootn_si","mpfr_root",
		"mpfr_neg","mpfr_abs","mpfr_dim","mpfr_mul_2ui","mpfr_mul_2si","mpfr_div_2ui","mpfr_div_2si","mpfr_fac_ui",
		"mpfr_fma","mpfr_fms","mpfr_fmma","mpfr_fmms","mpfr_hypot","mpfr_sum","mpfr_dot","mpfr_log",
		"mpfr_log_ui","mpfr_log2","mpfr_log10","mpfr_log1p","mpfr_log2p1","mpfr_log10p1","mpfr_exp","mpfr_exp2",
		"mpfr_exp10","mpfr_expm1","mpfr_exp2m1","mpfr_exp10m1","mpfr_pow","mpfr_powr","mpfr_pow_ui","mpfr_pow_si",
		"mpfr_pow_uj","mpfr_pow_sj","mpfr_pown","mpfr_pow_z","mpfr_ui_pow_ui","mpfr_ui_pow","mpfr_compound_si","mpfr_cos",
		"mpfr_sin","mpfr_tan","mpfr_cosu","mpfr_sinu","mpfr_tanu","mpfr_cospi","mpfr_sinpi","mpfr_tanpi",
		"mpfr_sin_cos","mpfr_sec","mpfr_csc","mpfr_cot","mpfr_acos","mpfr_asin","mpfr_atan","mpfr_acosu",
		"mpfr_asinu","mpfr_atanu","mpfr_acospi","mpfr_asinpi","mpfr_atanpi","mpfr_atan2","mpfr_atan2u","mpfr_atan2pi",
		"mpfr_cosh","mpfr_sinh","mpfr_tanh","mpfr_sinh_cosh","mpfr_sech","mpfr_csch","mpfr_coth","mpfr_acosh",
		"mpfr_asinh","mpfr_atanh","mpfr_eint","mpfr_li2","mpfr_gamma","mpfr_gamma_inc","mpfr_lngamma","mpfr_lgamma",
		"mpfr_digamma","mpfr_beta","mpfr_zeta","mpfr_zeta_ui","mpfr_erf","mpfr_erfc","mpfr_j0","mpfr_j1",
		"mpfr_jn","mpfr_y0","mpfr_y1","mpfr_yn","mpfr_agm","mpfr_ai","mpfr_const_log2","mpfr_const_pi",
		"mpfr_const_euler","mpfr_const_catalan","mpfr_rint","mpfr_rint_ceil","mpfr_rint_floor","mpfr_rint_round","mpfr_rint_roundeven","mpfr_rint_trunc",
		"mpfr_frac","mpfr_modf","mpfr_fmod","mpfr_fmod_ui","mpfr_fmodquo","mpfr_remainder","mpfr_remquo","mpfr_prec_round",
		"mpfr_can_round","mpfr_min","mpfr_max","mpfr_urandom","mpfr_nrandom","mpfr_grandom","mpfr_erandom","mpfr_setsign",
		"mpfr_copysign","mpfr_check_range","mpfr_subnormalize","mpfr_mul_2exp","mpfr_div_2exp"
	};

	private string GetReplacementForFunction(string comment, string body)
	{
		Match m = Regex.Match(comment, @"/// <returns>(.+)</returns>", RegexOptions.Singleline);
		if (!m.Success) return comment;

		if (knownSpecialApis.Any(api => body.Contains(api)))
		{
			return comment.Replace(m.Groups[0].Value,
			"""
			/// <returns>
			/// Returns a ternary value indicating the success of the operation.
			/// <para>If the value is 0, the result stored in the destination variable is exact.</para>
			/// <para>If the value is positive, the result is greater than the exact result, </para>
			/// <para>if the value is negative, the result is lower than the exact result. </para>
			/// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
			/// <para>A NaN result always corresponds to an exact return value. </para>
			/// <para>The opposite of the returned ternary value is representable in an int.</para>
			/// </returns>
			""");
		}
		else
		{
			return comment.Replace(m.Groups[0].Value,
			"""
			/// <returns>
			/// [UNSURE] Returns a ternary value indicating the success of the operation.
			/// <para>If the value is 0, the result stored in the destination variable is exact.</para>
			/// <para>If the value is positive, the result is greater than the exact result, </para>
			/// <para>if the value is negative, the result is lower than the exact result. </para>
			/// <para>If the result is infinite, it is considered inexact if it was obtained by overflow, and exact otherwise. </para>
			/// <para>A NaN result always corresponds to an exact return value. </para>
			/// <para>The opposite of the returned ternary value is representable in an int.</para>
			/// </returns>
			""");
		}
	}

	public static async Task<ClassDeclarationSyntax> ReplaceMethods(ClassDeclarationSyntax classDeclaration, CancellationToken cancellationToken = default)
	{
		var rewritter = new MethodReplacer();
		return (ClassDeclarationSyntax)rewritter.Visit(classDeclaration);
	}
}