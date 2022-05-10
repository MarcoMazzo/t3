﻿using System;
using System.Collections.Generic;
using Operators.Utils;
using T3.Core.Logging;
using T3.Core.Operator;
using T3.Gui.Graph;
using T3.Gui.Graph.Interaction;
using T3.Gui.Interaction.Variations.Midi;
using T3.Gui.Interaction.Variations.Model;
using T3.Gui.UiHelpers;
using T3.Gui.Windows.Variations;

//using T3.Gui.Windows.Variations;

namespace T3.Gui.Interaction.Variations
{
    /// <summary>
    /// Handles the live integration of variation model to the user interface.
    /// </summary>
    /// <remarks>
    /// Variations are a sets of symbolChild.input-parameters combinations defined for an Symbol.
    /// These input slots can also include the symbols out inputs which thus can be used for defining
    /// and applying "presets" to instances of that symbol.
    ///
    /// Most variations will modify(!) the symbol. This is great while working within a single symbol
    /// and tweaking an blending parameters. However it's potentially unintended (or dangerous) if the
    /// modified symbol has many instances. That's why applying symbol-variations is restricted for Symbols
    /// in the lib-namespace.  
    /// </remarks>
    public static class VariationHandling
    {
        public static SymbolVariationPool ActivePoolForSnapshots { get; private set; }
        public static Instance ActiveInstanceForSnapshots  { get; private set; }
        
        public static SymbolVariationPool ActivePoolForPresets { get; private set; }
        public static Instance ActiveInstanceForPresets  { get; private set; }

        public static void Init()
        {
            // Scan for output devices (e.g. to update LEDs etc.)
            MidiOutConnectionManager.Init();

            _inputDevices = new List<IControllerInputDevice>()
                                {
                                    new Apc40Mk2(),
                                    new NanoControl8(),
                                    new ApcMini(),
                                };
        }
        private static List<IControllerInputDevice> _inputDevices;
        
        /// <summary>
        /// Update variation handling
        /// </summary>
        public static void Update()
        {
            // Sync with composition selected in UI
            var primaryGraphWindow = GraphWindow.GetPrimaryGraphWindow();
            if (primaryGraphWindow == null)
                return;


            var singleSelectedInstance = NodeSelection.GetSelectedInstance();
            if (singleSelectedInstance != null)
            {
                var selectedSymbolId = singleSelectedInstance.Symbol.Id;
                ActivePoolForPresets = GetOrLoadVariations(selectedSymbolId);
                ActivePoolForSnapshots = GetOrLoadVariations(singleSelectedInstance.Parent.Symbol.Id);
                ActiveInstanceForPresets = singleSelectedInstance;
                ActiveInstanceForSnapshots = singleSelectedInstance.Parent;
            }
            else
            {
                ActivePoolForPresets = null;
                
                var activeCompositionInstance = primaryGraphWindow.GraphCanvas.CompositionOp;
                ActiveInstanceForSnapshots = activeCompositionInstance;
                
                // Prevent variations for library operators
                if (activeCompositionInstance.Symbol.Namespace.StartsWith("lib."))
                {
                    ActivePoolForSnapshots = null;
                }
                else
                {
                    ActivePoolForSnapshots = GetOrLoadVariations(activeCompositionInstance.Symbol.Id);
                }

                if (!NodeSelection.IsAnythingSelected())
                {
                    ActiveInstanceForPresets = ActiveInstanceForSnapshots;
                }
            }
            
            UpdateMidiDevices();
        }

        private static void UpdateMidiDevices()
        {
            // Update Midi Devices 
            foreach (var connectedDevice in _inputDevices)
            {
                // TODO: support generic input controllers with arbitrary DeviceId 
                var midiIn = MidiInConnectionManager.GetMidiInForProductNameHash(connectedDevice.GetProductNameHash());
                if (midiIn == null)
                    continue;

                if (ActivePoolForSnapshots != null)
                {
                    connectedDevice.Update(midiIn, ActivePoolForSnapshots.ActiveVariation);
                }
            }
        }
        
        

        public static SymbolVariationPool GetOrLoadVariations(Guid symbolId)
        {
            if (_variationPoolForOperators.TryGetValue(symbolId, out var variationForComposition))
            {
                return variationForComposition;
            }

            var newOpVariation = SymbolVariationPool.InitVariationPoolForSymbol(symbolId);
            _variationPoolForOperators[newOpVariation.SymbolId] = newOpVariation;
            return newOpVariation;
        }


        private static readonly Dictionary<Guid, SymbolVariationPool> _variationPoolForOperators = new();

        public static void ActivateOrCreatePresetAtIndex(int activationIndex)
        {
            if (ActivePoolForSnapshots == null)
            {
                Log.Warning($"Can't save variation #{activationIndex}. No variation pool active.");
                return;
            }
            
            if(SymbolVariationPool.TryGetSnapshot(activationIndex, out var existingVariation))
            {
                ActivePoolForSnapshots.Apply(ActiveInstanceForSnapshots, existingVariation, UserSettings.Config.PresetsResetToDefaultValues);
                return;
            } 
            
            CreateOrUpdateSnapshotVariation(activationIndex);
        }

        public static void SavePresetAtIndex(int activationIndex)
        {
            if (ActivePoolForSnapshots == null)
            {
                Log.Warning($"Can't save variation #{activationIndex}. No variation pool active.");
                return;
            }

            CreateOrUpdateSnapshotVariation(activationIndex);
        }

        public static void RemovePresetAtIndex(int activationIndex)
        {
            if (ActivePoolForSnapshots == null)
                return;
            
            //ActivePoolForSnapshots.DeleteVariation
            if (SymbolVariationPool.TryGetSnapshot(activationIndex, out var snapshot))
            {
                ActivePoolForSnapshots.DeleteVariation(snapshot);
            }
            else
            {
                Log.Warning($"No preset to delete at index {activationIndex}");
            }
        }

        public static void StartBlendingPresets(int[] indices)
        {
            Log.Warning($"StartBlendingPresets {indices} not implemented");
        }

        public static void BlendValuesUpdate(int obj)
        {
            Log.Warning($"BlendValuesUpdate {obj} not implemented");
        }

        public static void AppendPresetToCurrentGroup(int obj)
        {
            Log.Warning($"AppendPresetToCurrentGroup {obj} not implemented");
        }

        private const int AutoIndex=-1;
        public static Variation CreateOrUpdateSnapshotVariation(int activationIndex = AutoIndex )
        {
            // Only allow for snapshots.
            if (ActivePoolForSnapshots == null || ActiveInstanceForSnapshots == null)
            {
                return null;
            }
            
            // Delete previous snapshot for that index.
            if (activationIndex != AutoIndex && SymbolVariationPool.TryGetSnapshot(activationIndex, out var existingVariation))
            {
                ActivePoolForSnapshots.DeleteVariation(existingVariation);
            }
            
            _affectedInstances.Clear();
            if(FocusSetsForCompositions.TryGetValue(ActiveInstanceForSnapshots.Symbol.Id, out var filteredChildIds))
            {
                foreach (var child in ActiveInstanceForSnapshots.Children)
                {
                    if (filteredChildIds.Contains(child.SymbolChildId))
                    {
                        _affectedInstances.Add(child);    
                    }
                }

                // add new children...
                if (ChildIdsWhenFocusedForCompositions.TryGetValue(ActiveInstanceForSnapshots.Symbol.Id, out var previousChildIds))
                {
                    var countNewFocusItems = 0;
                    foreach (var child in ActiveInstanceForSnapshots.Children)
                    {
                        if (!previousChildIds.Contains(child.SymbolChildId))
                        {
                            countNewFocusItems++;
                            _affectedInstances.Add(child);    
                        }
                    }

                    if (countNewFocusItems > 0)
                    {
                        Log.Debug($"Added {countNewFocusItems} items added since setting focus to snapshot.");
                    }
                }
            }
            else
            {
                _affectedInstances.AddRange(ActiveInstanceForSnapshots.Children);
            }
            
            var newVariation = ActivePoolForSnapshots.CreateVariationForCompositionInstances(_affectedInstances);
            if (newVariation == null)
                return null;
            
            newVariation.PosOnCanvas = VariationBaseCanvas.FindFreePositionForNewThumbnail(VariationHandling.ActivePoolForSnapshots.Variations);
            if (activationIndex != AutoIndex)
                newVariation.ActivationIndex = activationIndex;
            
            return newVariation;
        }
        
        public static readonly Dictionary<Guid, HashSet<Guid>> FocusSetsForCompositions = new();
        /// <summary>
        /// This set can be used to find new children since the focus was set...
        /// </summary>
        public static readonly Dictionary<Guid, HashSet<Guid>> ChildIdsWhenFocusedForCompositions = new();
        private static readonly List<Instance> _affectedInstances = new(100);
    }
}