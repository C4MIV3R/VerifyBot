﻿using Discord;
using DL.GuildWars2Api;
using DL.GuildWars2Api.Models.V2;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Diagnostics;
using VerifyBot.Models;

namespace VerifyBot.Services
{
    public class VerifyService
    {
        
        private const int APIKeyLength = 72;
        private static readonly Regex AccountNameApiKeyRegex = new Regex(@"\s*(.+?\.\d+)\s+(.*?-.*?-.*?-.*?-.*)\s*$");
        private readonly UserStrings strings;

        public VerifyService(string accountId, string accountName, string apiKey, Manager manager, IUser requestor, UserStrings strings, IMessageChannel channel)
        {
            AccountId = accountId;
            AccountName = accountName;
            APIKey = apiKey;
            Requestor = requestor;
            Channel = channel;
            Manager = manager;

            this.strings = strings;

            API = new ApiFacade(APIKey);

            HasValidCharacter = false;

            IsValidAllianceUser = false;
        }

        public Account Account { get; private set; }

        public string AccountId { get; }

        public string AccountName { get; }

        public string APIKey { get; }

        public IMessageChannel Channel { get; }

        public int World
        {
            get
            {
                return this.Account?.WorldId ?? -999;
            }
        }

        public bool IsValid => IsValidAccount && IsValidAllianceUser; // && HasValidCharacter

        public IUser Requestor { get; }

        private ApiFacade API { get; }

        private bool HasValidCharacter { get; set; }

        private bool IsValidAllianceUser { get; set; }

        private bool IsValidAccount => Account != null;

        private Manager Manager { get; }

        public static VerifyService Create(string accountName, string apiKey, Manager manager, IUser requestor, UserStrings strings, IMessageChannel channel = null)
        {
            return new VerifyService(accountName, null, apiKey, manager, requestor, strings, channel);
        }

        public static async Task<VerifyService> CreateFromRequestMessage(IMessage requestMessage, Manager manager, UserStrings strings)
        {
            var tokens = AccountNameApiKeyRegex.Split(requestMessage.Content.Trim());

            if (tokens.Length != 4)
            {
                await requestMessage.Channel.SendMessageAsync(strings.ParseError);
                Console.WriteLine($"Could not verify {requestMessage.Author.Username} - Bad # of arguments");
                return null;
            }

            if (tokens[2].Length != APIKeyLength)
            {
                await requestMessage.Channel.SendMessageAsync(strings.InvalidAPIKey);
                Console.WriteLine($"Could not verify {requestMessage.Author.Username} - Bad API Key");
                return null;
            }

            return new VerifyService(null, tokens[1], tokens[2], manager, requestMessage.Author, strings, requestMessage.Channel);
        }

        public async Task<IUserMessage> SendMessageAsync(string message)
        {
            if (Channel == null)
                return null; // Task.FromResult<object>(null);
            return await Channel.SendMessageAsync(message);
        }

        public async Task Validate(bool isReverify)
        {
            await ValidateAccount(isReverify);
            if (IsValidAccount)
            {
                //await ValidateCharacters();
                await ValidateAlliance();
            }
        }

        public async Task LoadAccount()
        {
            var account = await API.V2.Authenticated.GetAccountAsync();

            if (account != null)
            {
                Account = account;
            }           
        }

        private async Task ValidateAccount(bool isReverify)
        {
            var account = await API.V2.Authenticated.GetAccountAsync();

            if (account == null)
            {
                await SendMessageAsync(this.strings.AccountNotInAPI);
                Console.WriteLine($"Could not verify {Requestor.Username} - Cannont access account in GW2 API.");
                return;
            }

            if (isReverify)
            {
                if (account.Id != AccountId)
                {                    
                    Console.WriteLine($"Could not verify {Requestor.Username} - API Key account does not match supplied account ID.");
                    return;
                }
            }
            else
            {
                if (account.Name != AccountName)
                {
                    await SendMessageAsync(this.strings.AccountNameDoesNotMatch);
                    Console.WriteLine($"Could not verify {Requestor.Username} - API Key account does not match supplied account. (Case matters)");
                    return;
                }
            }

            if (!Manager.IsAccountOnOurWorld(account))
            {
                await SendMessageAsync(this.strings.AccountNotOnServer);
                Console.WriteLine($"Could not verify {Requestor.Username} - Not on Server.");
                return;
            }
        
            Account = account;
        }

        private async Task ValidateCharacters()
        {           
            if (Account.Access.Count() == 0)
            {
                var characters = await API.V2.Authenticated.GetCharactersAsync();

                var isWvWLevel = false;
                foreach (var character in characters)
                {
                    var characterObj = await API.V2.Authenticated.GetCharacterAsync(character);

                    if (characterObj.Level >= 60)
                    {
                        isWvWLevel = true;
                        break;
                    }
                }

                if (!isWvWLevel)
                {
                    await SendMessageAsync(this.strings.NotValidLevel);
                    Console.WriteLine($"Could not verify {Requestor.Username} - Not elgible for WvW.");
                    return;
                }
            }

            HasValidCharacter = true;
        }

        // comment out when testing
        private async Task ValidateAlliance()
        {
            if (Account.GuildIds.Count() >= 1)
            {
                var isInAlliance = false;
                var userAccount = await API.V2.Authenticated.GetAccountAsync();
                var userGuilds = userAccount.GuildIds;

                foreach(var guild in userGuilds)
                {
                    if(Manager.Config.GuildIds.Contains(guild))
                    {
                        Console.WriteLine($"--- Guild ID = {guild} ---");
                        isInAlliance = true;
                        Console.WriteLine($"--- isInAlliance = {isInAlliance} ---");
                        break;
                    }
                }

                if(!isInAlliance)
                {
                    await SendMessageAsync(this.strings.AccountNotInAlliance);
                    Console.WriteLine($"Could not verify {Requestor.Username} - Not in our Alliance");
                    return;
                }
            }
            IsValidAllianceUser = true;
            Console.WriteLine($"--- IsValidAllianceUser = {IsValidAllianceUser} ---");
        }
    }
}