using System.Text;

namespace IronBrew2.Utilities
{
	public static class EncodingConstants
	{
		public static readonly Encoding LuaBytecodeEncoding = Encoding.GetEncoding(28591);

		static EncodingConstants()
		{
			Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
		}
	}
}
