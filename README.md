# VerifyBot   
[![Build Status](https://travis-ci.org/C4MIV3R/VerifyBot.svg?branch=master)](https://travis-ci.org/C4MIV3R/VerifyBot)

A bot that uses the Discord.NET and Guild Wars 2 API's to verify what world a users account is on. Made for a TC Alliance Discord but open source for all to use.   

You'll need a verified rank in Discord and a #verify text channel. When a user types !verify the bot will message them with instructions. Once a user is verified an entry is made into a SQLite database and the user is given the verified rank.

You'll need to add a secrets.json file to the directory that the executable is running from. Here are the required fields.
```
{
	"GuildIds": [ "0X0XX0XX-XX00-0000-X00X-0X0XXX00X000", "0X0XX0XX-XX00-0000-X00X-0X0XXX00X000", "0X0XX0XX-XX00-0000-X00X-0X0XXX00X000" ],
	"ServerId": your_discord_server_id_here,
	"DiscordToken": "your_discord_bot_token_here",
	"VerifyRole": "your_verified_role_name_here"
}
```
