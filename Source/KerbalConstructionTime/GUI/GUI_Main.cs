﻿using RP0;
using System.Collections.Generic;
using UnityEngine;

namespace KerbalConstructionTime
{
    public static partial class KCT_GUI
    {
        private const int _centralWindowWidth = 150;
        private const int _blPlusWidth = 100;
        
        public static GUIStates GUIStates = new GUIStates();
        public static GUIStates PrevGUIStates = null;
        public static GUIDataSaver GuiDataSaver = new GUIDataSaver();

        private static Rect _centralWindowPosition = new Rect((Screen.width - _centralWindowWidth * UIHolder.UIScale) / 2, (Screen.height - 50) / 2, _centralWindowWidth * UIHolder.UIScale, 1);
        private static Rect _blPlusPosition = new Rect(Screen.width - _blPlusWidth * UIHolder.UIScale, 40, _blPlusWidth * UIHolder.UIScale, 1);
        private static Vector2 _scrollPos;
        private static Vector2 _scrollPos2;
        private static GUIStyle _orangeText;

        private static bool _unlockEditor;
        private static bool _isKSCLocked = false;
        private static bool _inSCSubscene = false;
        public static bool InSCSubscene => _inSCSubscene;
        private static readonly List<GameScenes> _validScenes = new List<GameScenes> { GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPACECENTER, GameScenes.TRACKSTATION };
        private static GUIStyle _styleLabelRightAlign;
        private static GUIStyle _styleLabelCenterAlign;
        private static GUIStyle _styleTextFieldRightAlign;
        private static GUIStyle _styleLabelMultiline;

        public static bool IsPrimarilyDisabled => PresetManager.PresetLoaded() && (!PresetManager.Instance.ActivePreset.GeneralSettings.Enabled ||
                                                                                   !PresetManager.Instance.ActivePreset.GeneralSettings.BuildTimes);

        public static void SetGUIPositions()
        {
            if (_validScenes.Contains(HighLogic.LoadedScene))
            {
                if (GUIStates.ShowSettings)
                    _presetWindowPosition = DrawWindowWithTooltipSupport(_presetWindowPosition, "DrawPresetWindow", "Settings", DrawPresetWindow);
                if (!PresetManager.Instance.ActivePreset.GeneralSettings.Enabled)
                    return;

                if (GUIStates.ShowEditorGUI)
                    EditorWindowPosition = DrawWindowWithTooltipSupport(EditorWindowPosition, "DrawEditorGUI", "Integration Info", DrawEditorGUI);
                if (GUIStates.ShowSimulationGUI)
                    _simulationWindowPosition = DrawWindowWithTooltipSupport(_simulationWindowPosition, "DrawSimGUI", "Simulation", DrawSimulationWindow);
                if (GUIStates.ShowSimConfig)
                    _simulationConfigPosition = DrawWindowWithTooltipSupport(_simulationConfigPosition, "DrawSimConfGUI", "Simulation Configuration", DrawSimulationConfigure);
                if (GUIStates.ShowSimBodyChooser)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawSimBodyGUI", "Choose Body", DrawBodyChooser);
                if (GUIStates.ShowBuildList)
                {
                    ref Rect pos = ref (HighLogic.LoadedSceneIsEditor ? ref EditorBuildListWindowPosition : ref BuildListWindowPosition);
                    pos = DrawWindowWithTooltipSupport(pos, "DrawBuildListWindow", "Space Center Management", DrawBuildListWindow);
                }
                if (GUIStates.ShowClearLaunch)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawClearLaunch", "Launch site not clear!", DrawClearLaunch);
                if (GUIStates.ShowShipRoster)
                    _crewListWindowPosition = DrawWindowWithTooltipSupport(_crewListWindowPosition, "DrawShipRoster", "Select Crew", DrawShipRoster);
                if (GUIStates.ShowCrewSelect)
                    _crewListWindowPosition = DrawWindowWithTooltipSupport(_crewListWindowPosition, "DrawCrewSelect", "Select Crew & Launch", DrawCrewSelect);
                //if (GUIStates.ShowUpgradeWindow)
                //    _upgradePosition = DrawWindowWithTooltipSupport(_upgradePosition, "DrawUpgradeWindow", "Upgrades", DrawUpgradeWindow);
                if (GUIStates.ShowPersonnelWindow)
                    _personnelPosition = DrawWindowWithTooltipSupport(_personnelPosition, "DrawPersonnelWindow", "Staffing", DrawPersonnelWindow);
                if (GUIStates.ShowBLPlus)
                    _blPlusPosition = DrawWindowWithTooltipSupport(_blPlusPosition, "DrawBLPlusWindow", "Options", DrawBLPlusWindow);
                if (GUIStates.ShowDismantlePad)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawDismantlePadWindow", "Dismantle Pad", DrawDismantlePadWindow);
                if (GUIStates.ShowDismantleLC)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawDismantlePadWindow", "Dismantle Launch Complex", DrawDismantlePadWindow);
                if (GUIStates.ShowRename)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawRenameWindow", "Rename", DrawRenameWindow);
                if (GUIStates.ShowNewPad)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawNewPadWindow", "New Launch Pad", DrawNewPadWindow);
                if (GUIStates.ShowNewLC)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawNewLCWindow", "New Launch Complex", DrawNewLCWindow);
                if (GUIStates.ShowModifyLC)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawModifyLCWindow", "Modify Launch Complex", DrawNewLCWindow);
                if (GUIStates.ShowFirstRun)
                    _firstRunWindowPosition = DrawWindowWithTooltipSupport(_firstRunWindowPosition, "DrawFirstRun", "Space Center Setup", DrawFirstRun);
                if (GUIStates.ShowPresetSaver)
                    _presetNamingWindowPosition = DrawWindowWithTooltipSupport(_presetNamingWindowPosition, "DrawPresetSaveWindow", "Save as New Preset", DrawPresetSaveWindow);
                if (GUIStates.ShowLaunchSiteSelector)
                    _centralWindowPosition = DrawWindowWithTooltipSupport(_centralWindowPosition, "DrawLaunchSiteChooser", "Select Site", DrawLaunchSiteChooser);

                // Only show plans if we don't have a popup
                if (GUIStates.ShowBuildPlansWindow && !_isKSCLocked)
                    _buildPlansWindowPosition = DrawWindowWithTooltipSupport(_buildPlansWindowPosition, "DrawBuildPlansWindow", "Building Plans & Construction", DrawBuildPlansWindow);

                // both flags can be true when it's necessary to first show ClearLaunch and then Airlaunch right after that
                if (GUIStates.ShowAirlaunch && !GUIStates.ShowClearLaunch)
                    _airlaunchWindowPosition = DrawWindowWithTooltipSupport(_airlaunchWindowPosition, "DrawAirlaunchWindow", "Airlaunch", DrawAirlaunchWindow);

                if (_unlockEditor)
                {
                    EditorLogic.fetch.Unlock("KCTGUILock");
                    _unlockEditor = false;
                }

                if (HighLogic.LoadedSceneIsEditor)
                {
                    DoBuildPlansList();
                    CreateDevPartsToggle();
                }
                else if (_inSCSubscene)
                {
                    if (HighLogic.LoadedScene != GameScenes.SPACECENTER)
                        _inSCSubscene = false;
                }

                //Disable KSC things when certain windows are shown.
                if (GUIStates.ShowFirstRun || GUIStates.ShowRename || GUIStates.ShowNewPad || GUIStates.ShowNewLC || GUIStates.ShowModifyLC || GUIStates.ShowDismantleLC || GUIStates.ShowDismantlePad || GUIStates.ShowUpgradeWindow || GUIStates.ShowSettings || GUIStates.ShowCrewSelect || GUIStates.ShowShipRoster || GUIStates.ShowClearLaunch || GUIStates.ShowAirlaunch || GUIStates.ShowLaunchSiteSelector)
                {
                    if (!_isKSCLocked)
                    {
                        InputLockManager.SetControlLock(ControlTypes.KSC_FACILITIES, KerbalConstructionTime.KCTKSCLock);
                        _isKSCLocked = true;
                    }
                }
                else if (_isKSCLocked)
                {
                    InputLockManager.RemoveControlLock(KerbalConstructionTime.KCTKSCLock);
                    _isKSCLocked = false;
                }
            }
        }

        public static void ClickToggle()
        {
            ToggleVisibility(!GUIStates.IsMainGuiVisible);
        }

        public static void ToggleVisibility(bool isVisible)
        {
            if (KCTEvents.Instance.KCTButtonStockImportant)
                KCTEvents.Instance.KCTButtonStockImportant = false;

            if (HighLogic.LoadedScene == GameScenes.FLIGHT && !IsPrimarilyDisabled)
            {
                BuildListWindowPosition.height = 1;
                GUIStates.ShowBuildList = isVisible;
                GUIStates.ShowBLPlus = false;
                ResetBLWindow();

                if (Utilities.IsSimulationActive && (AirlaunchTechLevel.AnyUnlocked() || AirlaunchTechLevel.AnyUnderResearch()))
                {
                    GUIStates.ShowAirlaunch = isVisible;
                }
                if (KCTGameStates.IsSimulatedFlight)
                {
                    GUIStates.ShowSimulationGUI = isVisible;
                    _simulationWindowPosition.height = 1;
                }
            }
            else if ((HighLogic.LoadedScene == GameScenes.EDITOR) && !IsPrimarilyDisabled)
            {
                EditorWindowPosition.height = 1;
                GUIStates.ShowEditorGUI = isVisible;
                if (!isVisible)
                    GUIStates.ShowBuildList = false;
                KCTGameStates.ShowWindows[1] = isVisible;
            }
            else if ((HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.TRACKSTATION) && !IsPrimarilyDisabled)
            {
                BuildListWindowPosition.height = 1;
                GUIStates.ShowBuildList = isVisible;
                GUIStates.ShowBuildPlansWindow = false;
                GUIStates.ShowBLPlus = false;
                ResetBLWindow();
                KCTGameStates.ShowWindows[0] = isVisible;
            }

            RefreshToolbarState();
        }

        private static void RefreshToolbarState()
        {
            if (GUIStates.IsMainGuiVisible)
            {
                KCTGameStates.ToolbarControl.SetTrue(false);
            }
            else
            {
                KCTGameStates.ToolbarControl.SetFalse(false);
            }
        }

        public static void RestorePrevUIState()
        {
            if (PrevGUIStates == null) return;

            GUIStates = PrevGUIStates;
            PrevGUIStates = null;

            if (HighLogic.LoadedScene == GameScenes.SPACECENTER || HighLogic.LoadedScene == GameScenes.EDITOR)
            {
                int idx = HighLogic.LoadedScene == GameScenes.SPACECENTER ? 0 : 1;
                KCTGameStates.ShowWindows[idx] = GUIStates.IsMainGuiVisible;
            }

            RefreshToolbarState();
        }

        public static void BackupUIState()
        {
            PrevGUIStates = GUIStates.Clone();
        }

        public static void EnsureEditModeIsVisible()
        {
            GUIStates.ShowEditorGUI = true;
            RefreshToolbarState();
        }

        public static void OnRightClick()
        {
            if (HighLogic.LoadedScene == GameScenes.SPACECENTER && PresetManager.PresetLoaded() && !GUIStates.ShowFirstRun)
            {
                if (!GUIStates.ShowSettings)
                {
                    ShowSettings();
                }
                else
                {
                    GUIStates.ShowSettings = false;
                }
            }
        }

        public static void HideAll()
        {
            GUIStates.ShowEditorGUI = false;
            GUIStates.ShowBuildList = false;
            GUIStates.ShowClearLaunch = false;
            GUIStates.ShowShipRoster = false;
            GUIStates.ShowCrewSelect = false;
            GUIStates.ShowSettings = false;
            GUIStates.ShowUpgradeWindow = false;
            GUIStates.ShowPersonnelWindow = false;
            GUIStates.ShowBLPlus = false;
            GUIStates.ShowRename = false;
            GUIStates.ShowDismantlePad = false;
            GUIStates.ShowDismantleLC = false;
            GUIStates.ShowNewLC = false;
            GUIStates.ShowNewPad = false;
            GUIStates.ShowModifyLC = false;
            GUIStates.ShowFirstRun = false;
            GUIStates.ShowPresetSaver = false;
            GUIStates.ShowLaunchSiteSelector = false;
            GUIStates.ShowAirlaunch = false;

            ResetBLWindow();
        }

        public static void RemoveInputLocks() => InputLockManager.RemoveControlLock("KCTPopupLock");

        public static void ResetFormulaRateHolders()
        {
            _fundsCost = int.MinValue;
            _nodeRate = int.MinValue;
        }

        public static void CenterWindow(ref Rect window)
        {
            window.x = (float)((Screen.width - window.width) / 2.0);
            window.y = (float)((Screen.height - window.height) / 2.0);
        }

        /// <summary>
        /// Clamps a window to the screen
        /// </summary>
        /// <param name="window">The window Rect</param>
        /// <param name="strict">If true, none of the window can go past the edge.
        /// If false, half the window can. Defaults to false.</param>
        public static void ClampWindow(ref Rect window, bool strict = false)
        {
            if (strict)
            {
                if (window.x < 0)
                    window.x = 0;
                if (window.x + window.width > Screen.width)
                    window.x = Screen.width - window.width;

                if (window.y < 0)
                    window.y = 0;
                if (window.y + window.height > Screen.height)
                    window.y = Screen.height - window.height;
            }
            else
            {
                float halfW = window.width / 2;
                float halfH = window.height / 2;
                if (window.x + halfW < 0)
                    window.x = -halfW;
                if (window.x + halfW > Screen.width)
                    window.x = Screen.width - halfW;

                if (window.y + halfH < 0)
                    window.y = -halfH;
                if (window.y + halfH > Screen.height)
                    window.y = Screen.height - halfH;
            }
        }

        private static GUIStyle GetLabelRightAlignStyle()
        {
            if (_styleLabelRightAlign == null)
            {
                _styleLabelRightAlign = new GUIStyle(GUI.skin.label);
                _styleLabelRightAlign.alignment = TextAnchor.LowerRight;
            }
            return _styleLabelRightAlign;
        }

        private static GUIStyle GetLabelCenterAlignStyle()
        {
            if (_styleLabelCenterAlign == null)
            {
                _styleLabelCenterAlign = new GUIStyle(GUI.skin.label);
                _styleLabelCenterAlign.alignment = TextAnchor.LowerCenter;
            }
            return _styleLabelCenterAlign;
        }

        private static GUIStyle GetTextFieldRightAlignStyle()
        {
            if (_styleTextFieldRightAlign == null)
            {
                _styleTextFieldRightAlign = new GUIStyle(GUI.skin.textField);
                _styleTextFieldRightAlign.alignment = TextAnchor.LowerRight;
            }
            return _styleTextFieldRightAlign;
        }
        
        private static GUIStyle GetLabelMultilineStyle()
        {
            if (_styleLabelMultiline == null)
            {
                _styleLabelMultiline = new GUIStyle(GUI.skin.label);
                _styleLabelMultiline.fixedHeight = 0;
                _styleLabelMultiline.wordWrap = true;
            }
            return _styleLabelMultiline;
        }
        
        public static void EnterSCSubcene()
        {
            _inSCSubscene = true;
        }
        public static void ExitSCSubcene()
        {
            _inSCSubscene = false;
        }
        
        /// <summary>
        /// Resets all GUIStyles and Rects used in the UI so that UI scale can change instantly
        /// </summary>
        public static void ResetStylesAndPositions()
        {
            KerbalConstructionTime.ReinitializeUIs();
            
            _styleLabelRightAlign = null;
            _styleLabelCenterAlign = null;
            _styleTextFieldRightAlign = null;
            _styleLabelMultiline = null;
            
            ResetTooltipStyle();
            InitTooltips();
            
            PositionAndSizeBuildPlansIcon();
            PositionAndSizeDevPartsIcon();
            
            _centralWindowPosition.size = new Vector2(_centralWindowWidth * UIHolder.UIScale, 1);
            _blPlusPosition.size = new Vector2(_blPlusWidth * UIHolder.UIScale, 1);
            _airlaunchWindowPosition.size = new Vector2(_airLaunchWindowWidth * UIHolder.UIScale, 1);
            BuildListWindowPosition.size = new Vector2(_buildListWindowWidth * UIHolder.UIScale, 1);
            EditorBuildListWindowPosition.size = new Vector2(_editorBuildListWindowWidth * UIHolder.UIScale, 1);
            _crewListWindowPosition.size = new Vector2(_crewListWindowWidth * UIHolder.UIScale, 1);
            EditorWindowPosition.size = new Vector2(_editorWindowWidth * UIHolder.UIScale, 1);
            _firstRunWindowPosition.size = new Vector2(_firstRunWindowWidth * UIHolder.UIScale, 1);
            _personnelPosition.size = new Vector2(_personnelWindowWidth * UIHolder.UIScale, 1);
            _presetWindowPosition.size = new Vector2(_presetWindowWidth * UIHolder.UIScale, 1);
            _presetNamingWindowPosition.size = new Vector2(_presetNamingWindowWidth * UIHolder.UIScale, 1);
            _simulationWindowPosition.size = new Vector2(_simulationWindowWidth * UIHolder.UIScale, 1);
            _simulationConfigPosition.size = new Vector2(_simulationConfigWidth * UIHolder.UIScale, 1);
        }
    }
}

/*
    KerbalConstructionTime (c) by Michael Marvin, Zachary Eck

    KerbalConstructionTime is licensed under a
    Creative Commons Attribution-NonCommercial-ShareAlike 4.0 International License.

    You should have received a copy of the license along with this
    work. If not, see <http://creativecommons.org/licenses/by-nc-sa/4.0/>.
*/
