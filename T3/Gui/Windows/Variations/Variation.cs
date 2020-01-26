﻿using System;
using System.Collections.Generic;
using T3.Core;
using T3.Core.Operator;
using SharpDX;
using T3.Core.Logging;
using T3.Gui.Commands;
using T3.Gui.Graph.Interaction;
using T3.Gui.Selection;
using Vector2 = System.Numerics.Vector2;
using Vector3 = System.Numerics.Vector3;
using Vector4 = System.Numerics.Vector4;

namespace T3.Gui.Windows.Variations
{
    public class Variation
    {
        public GridCell GridCell;
        public bool ThumbnailNeedsUpdate;

        private Variation(Dictionary<VariationParameter, InputValue> valuesForParameters)
        {
            ValuesForParameters = valuesForParameters;
            _changeCommand = CreateChangeCommand();
            ThumbnailNeedsUpdate = true;
        }

        public Variation Clone()
        {
            return new Variation(new Dictionary<VariationParameter, InputValue>(ValuesForParameters));
        }

        public void ApplyValues()
        {
            _changeCommand.Do();
            InvalidateParameters();
        }

        public void RestoreValues()
        {
            _changeCommand.Undo();
            InvalidateParameters();
        }

        private void InvalidateParameters()
        {
            foreach (var param in ValuesForParameters.Keys)
            {
                param.InputSlot.DirtyFlag.Invalidate();
            }
        }

        public static Variation Mix(IEnumerable<VariationParameter> variationParameters,
                                    IReadOnlyCollection<Tuple<Variation, float>> neighboursAndWeights, float scatter,
                                    GridCell cell = new GridCell())
        {
            // Collect neighbours
            var valuesForParameters = new Dictionary<VariationParameter, InputValue>();
            var useDefault = (neighboursAndWeights.Count == 0);

            foreach (var param in variationParameters)
            {
                if (useDefault)
                {
                    if (param.OriginalValue is InputValue<float> value)
                    {
                        valuesForParameters.Add(param, value);
                    }
                    else if (param.OriginalValue is InputValue<Vector2> vec2Value)
                    {
                        valuesForParameters.Add(param, vec2Value);
                    }
                    else if (param.OriginalValue is InputValue<Vector3> vec3Value)
                    {
                        valuesForParameters.Add(param, vec3Value);
                    }
                    else if (param.OriginalValue is InputValue<Vector4> vec4Value)
                    {
                        valuesForParameters.Add(param, vec4Value);
                    }

                    continue;
                }

                if (param.Type == typeof(float))
                {
                    var value = 0f;
                    var sumWeight = 0f;
                    foreach (var neighbour in neighboursAndWeights)
                    {
                        var neighbourVariation = neighbour.Item1;
                        var matchingParam = neighbourVariation.ValuesForParameters[param];
                        if (matchingParam is InputValue<float> floatInput)
                        {
                            value += floatInput.Value * neighbour.Item2;
                            sumWeight += neighbour.Item2;
                        }
                    }

                    value *= 1f / sumWeight + ((float)Random.NextDouble() - 0.5f) * scatter;
                    value += Random.NextFloat(-scatter, scatter);
                    valuesForParameters.Add(param, new InputValue<float>(value));
                }

                if (param.Type == typeof(Vector2))
                {
                    var value = Vector2.Zero;
                    var sumWeight = 0f;
                    foreach (var neighbour in neighboursAndWeights)
                    {
                        var neighbourVariation = neighbour.Item1;
                        var matchingParam = neighbourVariation.ValuesForParameters[param];
                        if (matchingParam is InputValue<Vector2> typedInput)
                        {
                            value += typedInput.Value * neighbour.Item2;
                            sumWeight += neighbour.Item2;
                        }
                    }

                    value *= 1f / sumWeight;
                    value += new Vector2(
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter)
                                        );

                    valuesForParameters.Add(param, new InputValue<Vector2>(value));
                }

                if (param.Type == typeof(Vector2))
                {
                    var value = Vector2.Zero;
                    var sumWeight = 0f;
                    foreach (var neighbour in neighboursAndWeights)
                    {
                        var neighbourVariation = neighbour.Item1;
                        var matchingParam = neighbourVariation.ValuesForParameters[param];
                        if (matchingParam is InputValue<Vector2> typedInput)
                        {
                            value += typedInput.Value * neighbour.Item2;
                            sumWeight += neighbour.Item2;
                        }
                    }

                    value *= 1f / sumWeight;
                    value += new Vector2(
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter)
                                        );

                    valuesForParameters.Add(param, new InputValue<Vector2>(value));
                }

                if (param.Type == typeof(Vector3))
                {
                    var value = Vector3.Zero;
                    var sumWeight = 0f;
                    foreach (var neighbour in neighboursAndWeights)
                    {
                        var neighbourVariation = neighbour.Item1;
                        var matchingParam = neighbourVariation.ValuesForParameters[param];
                        if (matchingParam is InputValue<Vector3> typedInput)
                        {
                            value += typedInput.Value * neighbour.Item2;
                            sumWeight += neighbour.Item2;
                        }
                    }

                    value *= 1f / sumWeight;
                    value += new Vector3(
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter)
                                        );

                    valuesForParameters.Add(param, new InputValue<Vector3>(value));
                }

                if (param.Type == typeof(Vector4))
                {
                    var value = Vector4.Zero;
                    var sumWeight = 0f;
                    foreach (var neighbour in neighboursAndWeights)
                    {
                        var neighbourVariation = neighbour.Item1;
                        var matchingParam = neighbourVariation.ValuesForParameters[param];
                        if (matchingParam is InputValue<Vector4> typedInput)
                        {
                            value += typedInput.Value * neighbour.Item2;
                            sumWeight += neighbour.Item2;
                        }
                    }

                    value *= 1f / sumWeight;
                    value += new Vector4(
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter),
                                         Random.NextFloat(-scatter, scatter)
                                        );

                    valuesForParameters.Add(param, new InputValue<Vector4>(value));
                }
            }

            return new Variation(valuesForParameters)
                   {
                       GridCell = cell,
                   };
        }

        private MacroCommand CreateChangeCommand()
        {
            var commands = new List<ICommand>();

            foreach (var (param, value) in ValuesForParameters)
            {
                InputValue v = null;
                var newCommand = new ChangeInputValueCommand(param.Instance.Parent.Symbol, param.SymbolChildUi.Id, param.Input)
                                 {
                                     Value = value,
                                     OriginalValue = param.OriginalValue,
                                 };
                commands.Add(newCommand);
            }

            return new MacroCommand("Set Preset Values", commands);
            ;
        }

        public class VariationParameter
        {
            public List<Guid> InstanceIdPath = new List<Guid>();
            public Instance Instance => NodeOperations.GetInstanceFromIdPath(InstanceIdPath);
            public SymbolChildUi SymbolChildUi;
            public IInputSlot InputSlot { get; set; }
            public InputValue OriginalValue { get; set; }
            public SymbolChild.Input Input;
            public Type Type;
            public float Strength = 1;
        }

        public readonly Dictionary<VariationParameter, InputValue> ValuesForParameters;

        private static readonly Random Random = new Random();
        private ICommand _changeCommand;
    }
}