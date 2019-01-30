using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Linq;
using NAudio.Midi;
using System;
using System.IO;
using MidiPlayerTK;

namespace MidiPlayerTK
{
    /// <summary>
    /// Scan a midifile to returns patchs used
    /// </summary>
    public class PatchOptim
    {
        public int Bank;
        public bool Drum;
        public int Patch;
        public string Name;
        public bool Selected;

        static public List<PatchOptim> PatchUsed()
        {
            List<PatchOptim> filters = new List<PatchOptim>();
            try
            {
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    foreach (ImBank bank in MidiPlayerGlobal.ImSFCurrent.Banks)
                    {
                        if (bank != null && (MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.DefaultBankNumber == bank.BankNumber || MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.DrumKitBankNumber == bank.BankNumber))
                        {
                            foreach (ImPreset preset in bank.Presets)
                            {
                                if (preset != null)
                                {
                                    filters.Add(new PatchOptim() { Bank = bank.BankNumber, Patch = preset.Patch, Name = preset.Name, Selected = true, Drum= (MidiPlayerGlobal.CurrentMidiSet.ActiveSounFontInfo.DrumKitBankNumber == bank.BankNumber) });
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
            return filters;
        }
        static public void SfOptim(List<PatchOptim> filters)
        {
            try
            {
                if (MidiPlayerGlobal.ImSFCurrent != null)
                {
                    for (int b = 0; b < MidiPlayerGlobal.ImSFCurrent.Banks.Length; b++)
                    {
                        ImBank bank = MidiPlayerGlobal.ImSFCurrent.Banks[b];
                        if (bank != null)
                        {
                            for (int p = 0; p < bank.Presets.Length; p++)
                            {
                                ImPreset preset = bank.Presets[p];
                                if (preset != null)
                                {
                                    bool found = false;
                                    foreach (PatchOptim optim in filters)
                                        if (b == optim.Bank && p == optim.Patch && optim.Selected)
                                        {
                                            found = true;
                                            break;
                                        }
                                    if (!found)
                                        bank.Presets[p] = null;
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception ex)
            {
                MidiPlayerGlobal.ErrorDetail(ex);
            }
        }
    }
}

