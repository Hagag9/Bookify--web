using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace bookify.Web.Helpers
{
	[HtmlTargetElement("a",Attributes ="active-when")]
	public class ActiveTag:TagHelper
	{
		public string? ActiveWhen { get; set; }

		[ViewContext]
		[HtmlAttributeNotBound]
		public ViewContext? ViewContextData { get; set; }

		public override void Process(TagHelperContext context, TagHelperOutput output)
		{
			if (string.IsNullOrEmpty(ActiveWhen)) return;
			var CurrentControler = ViewContextData?.RouteData.Values["controller"]?.ToString();
			if(CurrentControler!.Equals(ActiveWhen))
			{
				var classAttribute = output.Attributes["class"].Value;
				var Classes = classAttribute == null ? "active" : classAttribute + "active";
				output.Attributes.SetAttribute("class", Classes);
			}

		}
	}
}
