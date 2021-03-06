﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Haasonline.Public.LocalApi.CSharp.DataObjects.CustomBots;
using Haasonline.Public.LocalApi.CSharp.Enums;
using Newtonsoft.Json;

namespace BruteScalp
{
    public static class BackTestHistoryManager
    {
        private static BackTestHistory backTestHistory;

        public static string DefaultBackTestHistoryFileName = "Backtest.history";

        public static bool LoadBacktestHistory()
        {
            if (File.Exists(DefaultBackTestHistoryFileName))
            {
                BackTestHistoryManager.backTestHistory = Newtonsoft.Json.JsonConvert.DeserializeObject<BackTestHistory>(File.ReadAllText(DefaultBackTestHistoryFileName));
                return true;
            }
            else
            {

                return false;
            }

        }

        public static bool SaveBackTestHistory()
        {
            try
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(BackTestHistoryManager.backTestHistory, Formatting.Indented);

                File.WriteAllText(DefaultBackTestHistoryFileName, json);

                return true;
            }
            catch
            {

                return false;
            }
        }

        public static bool DeleteBackTestHistory() {
            
            if (File.Exists(DefaultBackTestHistoryFileName))
            {
                File.Delete(DefaultBackTestHistoryFileName);
                return true;
            }

            return false;
        }

        public static void AddHistoryEntry(string accountGuid, decimal activationRoi, decimal normalBackTestValue, BaseCustomBot baseCustomBot)
        {
            BackTestData backTestData = new BackTestData();

            backTestData.AccountGUID = accountGuid;
            backTestData.Exchange = baseCustomBot.PriceMarket.PriceSource;
            backTestData.PrimaryCurrency = baseCustomBot.PriceMarket.PrimaryCurrency;
            backTestData.SecondayCurrency = baseCustomBot.PriceMarket.SecondaryCurrency;
            backTestData.ActivationROI = activationRoi;
            backTestData.StaticBackTestValue = normalBackTestValue;

            BackTestHistoryManager.backTestHistory.history.Add(backTestData);
        }

        public static void UpdateHistoryEntry(string accountGuid, decimal activationRoi, decimal normalBackTestValue, BaseCustomBot baseCustomBot)
        {

            foreach(var backTestEntry in BackTestHistoryManager.backTestHistory.history)
            {
                if(backTestEntry.AccountGUID.Equals(accountGuid) && 
                    backTestEntry.PrimaryCurrency == baseCustomBot.PriceMarket.PrimaryCurrency && 
                    backTestEntry.SecondayCurrency == baseCustomBot.PriceMarket.SecondaryCurrency)
                {
                    BackTestHistoryManager.RemoveHistoryEntry(backTestEntry);
                    BackTestHistoryManager.AddHistoryEntry(accountGuid, activationRoi, normalBackTestValue, baseCustomBot);
                    return;
                }
            }

            BackTestHistoryManager.AddHistoryEntry(accountGuid, activationRoi, normalBackTestValue, baseCustomBot);
        }

        public static void AddHistoryEntry(string accountGuid, EnumPriceSource priceSource, string primaryCurrency, string secondaryCurrency, decimal activationRoi, decimal observedHigh, decimal normalBackTestValue)
        {
            BackTestData backTestData = new BackTestData();

            backTestData.AccountGUID = accountGuid;
            backTestData.Exchange = priceSource;
            backTestData.PrimaryCurrency = primaryCurrency;
            backTestData.SecondayCurrency = secondaryCurrency;
            backTestData.ActivationROI = activationRoi;
            backTestData.ObservedHigh = observedHigh;
            backTestData.StaticBackTestValue = normalBackTestValue;

            BackTestHistoryManager.backTestHistory.history.Add(backTestData);
        }

        public static void UpdateHistoryEntry(string accountGuid, EnumPriceSource priceSource, string primaryCurrency, string secondaryCurrency, decimal activationRoi, decimal observedHigh, decimal normalBackTestValue)
        {

            foreach (var backTestEntry in BackTestHistoryManager.backTestHistory.history)
            {
                if (backTestEntry.AccountGUID.Equals(accountGuid) &&
                    backTestEntry.PrimaryCurrency == primaryCurrency &&
                    backTestEntry.SecondayCurrency == secondaryCurrency)
                {
                    BackTestHistoryManager.RemoveHistoryEntry(backTestEntry);
                    BackTestHistoryManager.AddHistoryEntry(accountGuid, priceSource, primaryCurrency, secondaryCurrency, activationRoi, observedHigh, normalBackTestValue);
                    return;
                }
            }

            BackTestHistoryManager.AddHistoryEntry(accountGuid, priceSource, primaryCurrency, secondaryCurrency, activationRoi, observedHigh, normalBackTestValue);

        }

        public static void RemoveHistoryEntry(BackTestData entry)
        {

            try
            { 
                    backTestHistory.history.Remove(entry);
            }
            catch
            {

           
            }
        }

        public static BackTestHistory GetAllHistory()
        {
            return BackTestHistoryManager.backTestHistory;
        }

        public static List<BackTestData> GetHistoryForAccount(string accountGUID)
        {
            List<BackTestData> results = new List<BackTestData>();

            foreach (var backTestEntry in BackTestHistoryManager.backTestHistory.history)
            {
                if(backTestEntry.AccountGUID.Equals(accountGUID))
                {
                    results.Add(backTestEntry);
                }
            }

            return results;

        }

        public static BackTestData GetHistoryForBot(BaseCustomBot baseCustomBot)
        {
            foreach (var backTestEntry in BackTestHistoryManager.backTestHistory.history)
            {
                if (backTestEntry.AccountGUID.Equals(ConfigManager.mainConfig.AccountGUID) &&
                    backTestEntry.PrimaryCurrency == baseCustomBot.PriceMarket.PrimaryCurrency &&
                    backTestEntry.SecondayCurrency == baseCustomBot.PriceMarket.SecondaryCurrency)
                {
                    return backTestEntry;
                }
            }

            return null;
        }

        public static BackTestData GetHistoryForMarket(string accountGuid, List<BackTestData> backTestHistory, Tuple<string, string> market)
        {
            foreach (var bth in backTestHistory)
            {
                if (bth.AccountGUID.Equals(accountGuid) && bth.PrimaryCurrency == market.Item1 && bth.SecondayCurrency == market.Item2)
                {
                    return bth;
                }
            }

            return null;
        }

        public static bool PerformStartup()
        {
            if (BackTestHistoryManager.LoadBacktestHistory())
            {
                return true;
            }
            else
            {
                BackTestHistoryManager.backTestHistory = new BackTestHistory();
                return false;
            }
        }
    }
}
