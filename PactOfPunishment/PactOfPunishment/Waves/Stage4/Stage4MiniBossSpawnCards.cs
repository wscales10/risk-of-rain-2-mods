using RoR2;

namespace PactOfPunishment.Waves.Stage4
{
    public class Stage4AddsSpawnCards
    {
        public readonly AssetPromise<CharacterSpawnCard> invalidatorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/DefectiveUnit/cscDefectiveUnit.asset");
        public readonly AssetPromise<CharacterSpawnCard> lemurianSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Lemurian/cscLemurian.asset");
        public readonly AssetPromise<CharacterSpawnCard> larvaSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC1/AcidLarva/cscAcidLarva.asset");
        public readonly AssetPromise<CharacterSpawnCard> geepSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC1/Gup/cscGeepBody.asset");

        private static Stage4AddsSpawnCards? instance;

        private Stage4AddsSpawnCards()
        {
        }

        public static Stage4AddsSpawnCards Instance => instance ??= new Stage4AddsSpawnCards();
    }

    public class Stage4MiniBossSpawnCards
    {
        public readonly AssetPromise<CharacterSpawnCard> invalidatorSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC3/DefectiveUnit/cscDefectiveUnit.asset");
        public readonly AssetPromise<CharacterSpawnCard> aurelioniteSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/Titan/cscTitanGold.asset");
        public readonly AssetPromise<CharacterSpawnCard> elderLemurianSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/Base/LemurianBruiser/cscLemurianBruiser.asset");
        public readonly AssetPromise<CharacterSpawnCard> gupSpawnCard = Utils.BeginLoad<CharacterSpawnCard>("RoR2/DLC1/Gup/cscGupBody.asset");

        private static Stage4MiniBossSpawnCards? instance;

        private Stage4MiniBossSpawnCards()
        {
        }

        public static Stage4MiniBossSpawnCards Instance => instance ??= new Stage4MiniBossSpawnCards();
    }
}