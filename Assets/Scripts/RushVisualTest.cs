using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
namespace Aether {
 public class RushVisualTest:MonoBehaviour {
  Game game;string output;
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]static void Boot(){if(Environment.GetCommandLineArgs().Contains("-aetherRushVisualTest"))new GameObject("Rush visual verification").AddComponent<RushVisualTest>();}
  IEnumerator Shot(string name){yield return new WaitForEndOfFrame();ScreenCapture.CaptureScreenshot(Path.Combine(output,name+".png"));yield return null;}
  IEnumerator Start(){output=Path.GetFullPath(Application.dataPath+"/../../Evidence/Rush");yield return new WaitForSeconds(1);game=FindAnyObjectByType<Game>();game.ChooseDevice(false);yield return Shot("01-rush-menu");game.menu="Inventory";yield return Shot("02-inventory");game.menu="Garage";yield return Shot("10-garage");game.ChooseDevice(true);game.StartRush(2);yield return new WaitUntil(()=>game.state==Game.State.Race);game.player.manualTest=true;yield return new WaitForSeconds(1);game.PauseRace();game.HandleBack();yield return Shot("08-phone-rush");Screen.SetResolution(1280,720,FullScreenMode.Windowed);yield return new WaitForSeconds(.4f);yield return Shot("11-phone-1280");Screen.SetResolution(960,540,FullScreenMode.Windowed);yield return new WaitForSeconds(.4f);yield return Shot("12-phone-960");game.PauseRace();yield return Shot("13-phone-pause-960");game.HandleBack();yield return new WaitForSeconds(7);Debug.Log("RUSH VISUAL COMPLETE");Application.Quit();}
 }
}
