using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class NewDayRecipe : MonoBehaviour
{
    public Sprite[] dailyRecipeSprites;
    public SceneInfo sceneInfo;
    public Sprite currentSprite;

    void Update()
    {
        switch (sceneInfo.dayCount)
        {


            case 0:
                currentSprite = dailyRecipeSprites[0];
                break;
            case 2:
                currentSprite = dailyRecipeSprites[1];
                break;
            case 3:
                currentSprite = dailyRecipeSprites[2];
                break;
            case 4:
                currentSprite = dailyRecipeSprites[3];
                break;
        }

        GetComponent<Image>().sprite = currentSprite;
    }
}
