using System;
using System.Collections.Generic;
using UnityEngine.Localization.SmartFormat.Core.Extensions;
using UnityEngine.Localization.SmartFormat.Core.Parsing;

namespace UnityEngine.Localization.SmartFormat.Extensions
{
    /// <summary>
    /// Provides the ability to add logic to a Smart String by selecting an output using a provided set of choices.
    /// </summary>
    [Serializable]
    public class ChooseAnyFormatter : FormatterBase, IFormatterLiteralExtractor
    {
        [SerializeField]
        char m_SplitChar = '|';

        /// <summary>
        /// The character used to split the choices.
        /// By default this is the pipe chartacter |.
        /// </summary>
        public char SplitChar
        {
            get => m_SplitChar;
            set => m_SplitChar = value;
        }

        /// <summary>
        /// Creates a new instance of the formatter.
        /// </summary>
        public ChooseAnyFormatter()
        {
            Names = DefaultNames;
        }

        /// <inheritdoc/>
        public override string[] DefaultNames => new[] { "chooseany", "ca", "any" };

        /// <inheritdoc/>
        public override bool TryEvaluateFormat(IFormattingInfo formattingInfo)
        {
            //if (formattingInfo.FormatterOptions == "") return false;
            var chooseOptions = formattingInfo.FormatterOptions.Split(SplitChar);
            var formats = formattingInfo.Format.Split(SplitChar);
            //if (formats.Count < 2) return false; 

            string floatPercent = formattingInfo.FormatterOptions;

            if (floatPercent != "")
            {
                float parsed = 100.0f;
                if (float.TryParse(floatPercent, out parsed))
                {
                    if (RogueRNG.Linear(0, 100f) > parsed)
                    {
                        formattingInfo.Write("");
                        return true;
                    }
                }
            }

            var chosenFormat = DetermineChosenFormat(formattingInfo, formats, chooseOptions);

            formattingInfo.Write(chosenFormat, formattingInfo.CurrentValue);

            return true;
        }

        private static Format DetermineChosenFormat(IFormattingInfo formattingInfo, IList<Format> choiceFormats, string[] chooseOptions)
        {
            var chosenIndex = RogueRNG.Linear(choiceFormats.Count);

            if (chosenIndex == -1) chosenIndex = choiceFormats.Count - 1;

            var chosenFormat = choiceFormats[RogueRNG.Linear(choiceFormats.Count)];
            return chosenFormat;
        }

        /// <inheritdoc/>
        public void WriteAllLiterals(IFormattingInfo formattingInfo)
        {
            if (formattingInfo.FormatterOptions == "")
                return;

            var formats = formattingInfo.Format.Split(SplitChar);
            if (formats.Count < 2)
                return;

            for (int i = 0; i < formats.Count; ++i)
            {
                formattingInfo.Write(formats[i], null);
            }
        }
    }
}
