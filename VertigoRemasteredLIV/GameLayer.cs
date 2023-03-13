namespace VertigoRemasteredLIV
{
	public enum GameLayer
	{
        Default = 0,
        TransparentFX = 1,
        IgnoreRaycast = 2,
        Water = 4,
        UI = 5,
        PlayerBody = 22, //Except arms
        VRRenderingOnly = 29,

        // Custom layers to use in the mod.
        LivOnly = 31 //Postprocessing
    }
}