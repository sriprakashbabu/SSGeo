using UnityEngine;
using TMPro;

public class SeasonUIController : MonoBehaviour
{
    public EarthSimulationController sim;
    public TMP_Text infoBox;

    public void GoToMarch()
    {
        sim.SwitchSeason(EarthSeason.March);
        if (infoBox) infoBox.text = "Spring in North, Autumn in South. Equal day & night.";
    }

    public void GoToJune()
    {
        sim.SwitchSeason(EarthSeason.June);
        if (infoBox) infoBox.text = "Summer in North, Winter in South. Sun at Tropic of Cancer.";
    }

    public void GoToSeptember()
    {
        sim.SwitchSeason(EarthSeason.September);
        if (infoBox) infoBox.text = "Autumn in North, Spring in South. Equal day & night.";
    }

    public void GoToDecember()
    {
        sim.SwitchSeason(EarthSeason.December);
        if (infoBox) infoBox.text = "Winter in North, Summer in South. Sun at Tropic of Capricorn.";
    }

    public void BackToOrbit()
    {
        sim.BackToOrbit();
        if (infoBox) infoBox.text = ""; // clear info text
    }
}
