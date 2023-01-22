using MyRoR2;
using System.Collections.Generic;

namespace WPFApp.ViewModels
{
	public class EntityPatternViewModel : ImagePatternViewModel<DefinedEntity>
	{
		public override IEnumerable<DefinedEntity> AllowedValues { get; } = DefinedEntity.GetAll();

		public override string GetDisplayName(DefinedEntity value) => value?.DisplayName;

		public override string GetTypeableName(DefinedEntity value) => value?.TypeableName;

		protected override string GetImgXPathFromTableXPath(string tableXPath, string typeableName) => $"{tableXPath}//img[contains(@data-image-name, '{typeableName}')]";
	}
}