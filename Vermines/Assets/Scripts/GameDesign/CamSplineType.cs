/// <summary>
/// Add some possible spline type before MainViewToCourtyard that are not shops is ok with the actual configuration.
/// Spline will be main view to location only compared to shop that can use splines shop to shop.
/// </summary>
public enum CamSplineType
{
    None = 0,
    MainViewToSacrifice,
    MainViewToCourtyard,
    MainViewToMarket,
    MarketToCourtyard,
    CourtyardToMarket,
    Count
}
