using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Linq;
using System.Collections.Generic;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.LayoutEngine;
using System.Runtime.InteropServices;

namespace OnTheFlyTranslator.Translation
{
    internal class TranslationService : IDisposable
    {
        private static TranslationService? Instance;
        public readonly TranslationDatabase<Lumina.Excel.Sheets.Action> actionDatabase;

        public TranslationService()
        {
            actionDatabase = new();
        }

        public void Dispose()
        {

        }

        public void DrawDebug()
        {
            if (!Configuration.DrawDebugWindow)
                return;
            InternalDrawDebug();
        }

        internal bool GetActionIDFromName(string strName, ref uint actionId, bool bFromOriginalLanguage)
        {
            var sheet = actionDatabase.GetSheet(bFromOriginalLanguage);
            actionId = sheet?.FirstOrDefault(data => data.Name.ToString() == strName).RowId ?? 0;
            return actionId != 0;
        }

        public TranslationResult? GetActionTranslation(uint actionId)
        {
            var data = actionDatabase.GetAvailableTranslation(actionId);
            return new TranslationResult(data.Original?.Name.ToString() ?? "", data.Target?.Name.ToString() ?? "");
        }

        public static TranslationService GetInstance() => Instance ??= new TranslationService();


        ////////////////// ---- DEBUGGING HERE ----- //////////////////
        private static TranslationResult? LastTranslationResult = null;
        private static uint TargetActionId = 0;
        private static string TargetActionName = "";
        private static bool LastActionSearchedFound = true;
        private static bool FromOriginalLanguage = true;

        private void InternalDrawDebug()
        {
            if (ImGui.Begin("Database", ref Configuration.DrawDebugWindow))
            {
                if (ImGui.BeginTabBar("Database Tabs"))
                {
                    if (ImGui.BeginTabItem("Translation search"))
                    {
                        DrawActionSearch();
                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Action list"))
                    {
                        DrawActionSheet();
                        ImGui.EndTabItem();
                    }
                }
                ImGui.End();
            }
        }

        private static List<Lumina.Excel.Sheets.Action>? SearchSheet;
        private static int LIST_SEARCH_LIMIT = 50;
        private void DrawActionSheet()
        {
            var bRefreshList = false;
            bRefreshList |= ImGui.Checkbox("From client language", ref FromOriginalLanguage);
            ImGui.InputInt("Element in list", ref LIST_SEARCH_LIMIT);
            if (ImGui.TreeNodeEx("Action database"))
            {
                bRefreshList |= ImGui.InputText("Search action", ref TargetActionName, 1024);
                if(bRefreshList || SearchSheet == null)
                {
                    SearchSheet = actionDatabase.GetSheet(FromOriginalLanguage)?.ToList();
                    SearchSheet = actionDatabase.GetSheet(FromOriginalLanguage)?.Where(element => element.Name.ToString().Contains(TargetActionName) || element.RowId.ToString().Contains(TargetActionName)).ToList();
                }

                if (ImGui.BeginTable("Translation", 3, ImGuiTableFlags.Borders))
                {
                    var sheet = SearchSheet?.Where(element => element.Name.ToString().Contains(TargetActionName) || element.RowId.ToString().Contains(TargetActionName));
                    if (sheet != null)
                    {
                        ImGui.Text("Found element: " + sheet.Count());
                        ImGui.TableSetupColumn("Action name");
                        ImGui.TableSetupColumn("ID");
                        ImGui.TableSetupColumn("Translated name");
                        ImGui.TableHeadersRow();
                        ImGui.TableNextRow();

                        var nCurrentSize = 0;
                        foreach (var item in sheet)
                        {
                            ImGui.TableSetColumnIndex(0);
                            ImGui.Text(item.Name.ToString());
                            ImGui.TableSetColumnIndex(1);
                            ImGui.Text(item.RowId.ToString());
                            ImGui.TableSetColumnIndex(2);

                            var data = actionDatabase.GetAvailableTranslation(item.RowId);
                            var translationResult = new TranslationResult(data.Original?.Name.ToString() ?? "", data.Target?.Name.ToString() ?? "");
                            ImGui.Text(FromOriginalLanguage ? translationResult.TranslatedName : translationResult.OriginalName);
                            ImGui.TableNextRow();
                            nCurrentSize++;

                            if (nCurrentSize >= LIST_SEARCH_LIMIT)
                                break;
                        }
                    }
                    ImGui.EndTable();
                }
            }
        }

        private void DrawActionSearch()
        {
            ImGui.Checkbox("Search in client language sheet", ref FromOriginalLanguage);
            ImGui.InputText("Target action name", ref TargetActionName, 1024);
            if (ImGui.Button("Fetch action ID"))
            {
                LastActionSearchedFound = GetActionIDFromName(TargetActionName, ref TargetActionId, FromOriginalLanguage);
            }

            if (!LastActionSearchedFound)
            {
                ImGui.TextColored(ImGuiColors.DalamudRed, "Last search action failed");
            }

            var actionId = (int)TargetActionId;
            if(ImGui.InputInt("Action ID to search", ref actionId, 1))
            {
                TargetActionId = (uint)actionId;
            }

            if (ImGui.Button("Fetch translation of ID: " + TargetActionId))
            {
                LastTranslationResult = GetActionTranslation(TargetActionId);
            }

            if (LastTranslationResult != null)
            {
                ImGui.Text("Source: " + LastTranslationResult.OriginalName + " / Target: " + LastTranslationResult.TranslatedName);
            }
        }
    }
}


