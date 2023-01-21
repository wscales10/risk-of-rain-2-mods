using MyRoR2;
using System.Collections.Generic;
using System.Net;

namespace WPFApp.ViewModels
{
	public class ScenePatternViewModel : ImagePatternViewModel<DefinedScene>
    {
        public override IEnumerable<DefinedScene> AllowedValues { get; } = DefinedScene.GetAll();

        public override string GetDisplayName(DefinedScene value) => value?.DisplayName;

        protected override string GetImgXPathFromTableXPath(string tableXPath, string typeableName) => $"{tableXPath}//img[@data-image-key='{WebUtility.UrlEncode(typeableName.Replace(' ', '_'))}.png']";
    }
}