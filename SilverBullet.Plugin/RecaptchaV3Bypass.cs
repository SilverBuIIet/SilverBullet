﻿using System;
using System.Text.RegularExpressions;
using System.Windows.Media;
using Extreme.Net;
using PluginFramework;
using PluginFramework.Attributes;
using RuriLib;
using RuriLib.LS;

namespace SilverBullet.Plugin
{
    internal class RecaptchaV3Bypass : BlockBase, IBlockPlugin
    {
        public RecaptchaV3Bypass()
        {
            Label = nameof(RecaptchaV3Bypass);
        }

        public string Name => nameof(RecaptchaV3Bypass);

        public LinearGradientBrush LinearGradientBrush => new LinearGradientBrush()
        {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(1, 1),
            GradientStops = new GradientStopCollection()
            {
                new GradientStop(){ Offset=0.3,Color= ColorConverter("#DB4437")},
                new GradientStop(){ Offset=0.1,Color= ColorConverter("#F4B400")},
                new GradientStop(){ Offset=0.87,Color= ColorConverter("#0F9D58")}
            }
        };

        public bool LightForeground => false;

        private string variableName;
        [Text("VariableName:")]
        public string VariableName
        {
            get { return variableName; }
            set
            {
                variableName = value;
                OnPropertyChanged();
            }
        }

        private string recaptchaUrlGet = "";
        [Text("Recaptcha Url (GET):")]
        public string RecaptchaUrlGet
        {
            get { return recaptchaUrlGet; }
            set
            {
                recaptchaUrlGet = value;
                OnPropertyChanged();
            }
        }

        private string bg = "!q62grYxHRvVxjUIjSFNd0mlvrZ-iCgIHAAAB6FcAAAANnAkBySdqTJGFRK7SirleWAwPVhv9-XwP8ugGSTJJgQ46-0IMBKN8HUnfPqm4sCefwxOOEURND35prc9DJYG0pbmg_jD18qC0c-lQzuPsOtUhHTtfv3--SVCcRvJWZ0V3cia65HGfUys0e1K-IZoArlxM9qZfUMXJKAFuWqZiBn-Qi8VnDqI2rRnAQcIB8Wra6xWzmFbRR2NZqF7lDPKZ0_SZBEc99_49j07ISW4X65sMHL139EARIOipdsj5js5JyM19a2TCZJtAu4XL1h0ZLfomM8KDHkcl_b0L-jW9cvAe2K2uQXKRPzruAvtjdhMdODzVWU5VawKhpmi2NCKAiCRUlJW5lToYkR_X-07AqFLY6qi4ZbJ_sSrD7fCNNYFKmLfAaxPwPmp5Dgei7KKvEQmeUEZwTQAS1p2gaBmt6SCOgId3QBfF_robIkJMcXFzj7R0G-s8rwGUSc8EQzT_DCe9SZsJyobu3Ps0-YK-W3MPWk6a69o618zPSIIQtSCor9w_oUYTLiptaBAEY03NWINhc1mmiYu2Yz5apkW_KbAp3HD3G0bhzcCIYZOGZxyJ44HdGsCJ-7ZFTcEAUST-aLbS-YN1AyuC7ClFO86CMICVDg6aIDyCJyIcaJXiN-bN5xQD_NixaXatJy9Mx1XEnU4Q7E_KISDJfKUhDktK5LMqBJa-x1EIOcY99E-eyry7crf3-Hax3Uj-e-euzRwLxn2VB1Uki8nqJQVYUgcjlVXQhj1X7tx4jzUb0yB1TPU9uMBtZLRvMCRKvFdnn77HgYs5bwOo2mRECiFButgigKXaaJup6NM4KRUevhaDtnD6aJ8ZWQZTXz_OJ74a_OvPK9eD1_5pTG2tUyYNSyz-alhvHdMt5_MAdI3op4ZmcvBQBV9VC2JLjphDuTW8eW_nuK9hN17zin6vjEL8YIm_MekB_dIUK3T1Nbyqmyzigy-Lg8tRL6jSinzdwOTc9hS5SCsPjMeiblc65aJC8AKmA5i80f-6Eg4BT305UeXKI3QwhI3ZJyyQAJTata41FoOXl3EF9Pyy8diYFK2G-CS8lxEpV7jcRYduz4tEPeCpBxU4O_KtM2iv4STkwO4Z_-c-fMLlYu9H7jiFnk6Yh8XlPE__3q0FHIBFf15zVSZ3qroshYiHBMxM5BVQBOExbjoEdYKx4-m9c23K3suA2sCkxHytptG-6yhHJR3EyWwSRTY7OpX_yvhbFri0vgchw7U6ujyoXeCXS9N4oOoGYpS5OyFyRPLxJH7yjXOG2Play5HJ91LL6J6qg1iY8MIq9XQtiVZHadVpZVlz3iKcX4vXcQ3rv_qQwhntObGXPAGJWEel5OiJ1App7mWy961q3mPg9aDEp9VLKU5yDDw1xf6tOFMwg2Q-PNDaKXAyP_FOkxOjnu8dPhuKGut6cJr449BKDwbnA9BOomcVSztEzHGU6HPXXyNdZbfA6D12f5lWxX2B_pobw3a1gFLnO6mWaNRuK1zfzZcfGTYMATf6d7sj9RcKNS230XPHWGaMlLmNxsgXkEN7a9PwsSVwcKdHg_HU4vYdRX6vkEauOIwVPs4dS7yZXmtvbDaX1zOU4ZYWg0T42sT3nIIl9M2EeFS5Rqms_YzNp8J-YtRz1h5RhtTTNcA5jX4N-xDEVx-vD36bZVzfoMSL2k85PKv7pQGLH-0a3DsR0pePCTBWNORK0g_RZCU_H898-nT1syGzNKWGoPCstWPRvpL9cnHRPM1ZKemRn0nPVm9Bgo0ksuUijgXc5yyrf5K49UU2J5JgFYpSp7aMGOUb1ibrj2sr-D63d61DtzFJ2mwrLm_KHBiN_ECpVhDsRvHe5iOx_APHtImevOUxghtkj-8RJruPgkTVaML2MEDOdL_UYaldeo-5ckZo3VHss7IpLArGOMTEd0bSH8tA8CL8RLQQeSokOMZ79Haxj8yE0EAVZ-k9-O72mmu5I0wH5IPgapNvExeX6O1l3mC4MqLhKPdOZOnTiEBlSrV4ZDH_9fhLUahe5ocZXvXqrud9QGNeTpZsSPeIYubeOC0sOsuqk10sWB7NP-lhifWeDob-IK1JWcgFTytVc99RkZTjUcdG9t8prPlKAagZIsDr1TiX3dy8sXKZ7d9EXQF5P_rHJ8xvmUtCWqbc3V5jL-qe8ANypwHsuva75Q6dtqoBR8vCE5xWgfwB0GzR3Xi_l7KDTsYAQIrDZVyY1UxdzWBwJCrvDrtrNsnt0S7BhBJ4ATCrW5VFPqXyXRiLxHCIv9zgo-NdBZQ4hEXXxMtbem3KgYUB1Rals1bbi8X8MsmselnHfY5LdOseyXWIR2QcrANSAypQUAhwVpsModw7HMdXgV9Uc-HwCMWafOChhBr88tOowqVHttPtwYorYrzriXNRt9LkigESMy1bEDx79CJguitwjQ9IyIEu8quEQb_-7AEXrfDzl_FKgASnnZLrAfZMtgyyddIhBpgAvgR_c8a8Nuro-RGV0aNuunVg8NjL8binz9kgmZvOS38QaP5anf2vgzJ9wC0ZKDg2Ad77dPjBCiCRtVe_dqm7FDA_cS97DkAwVfFawgce1wfWqsrjZvu4k6x3PAUH1UNzQUxVgOGUbqJsaFs3GZIMiI8O6-tZktz8i8oqpr0RjkfUhw_I2szHF3LM20_bFwhtINwg0rZxRTrg4il-_q7jDnVOTqQ7fdgHgiJHZw_OOB7JWoRW6ZlJmx3La8oV93fl1wMGNrpojSR0b6pc8SThsKCUgoY6zajWWa3CesX1ZLUtE7Pfk9eDey3stIWf2acKolZ9fU-gspeACUCN20EhGT-HvBtNBGr_xWk1zVJBgNG29olXCpF26eXNKNCCovsILNDgH06vulDUG_vR5RrGe5LsXksIoTMYsCUitLz4HEehUOd9mWCmLCl00eGRCkwr9EB557lyr7mBK2KPgJkXhNmmPSbDy6hPaQ057zfAd5s_43UBCMtI-aAs5NN4TXHd6IlLwynwc1zsYOQ6z_HARlcMpCV9ac-8eOKsaepgjOAX4YHfg3NekrxA2ynrvwk9U-gCtpxMJ4f1cVx3jExNlIX5LxE46FYIhQ";
        [Text("BG:")]
        public string BG
        {
            get { return bg; }
            set
            {
                bg = value;
                OnPropertyChanged();
            }
        }

        private string recaptchaUrlPost = "";
        [Text("Recaptcha Url (POST):")]
        public string RecaptchaUrlPost
        {
            get { return recaptchaUrlPost; }
            set
            {
                recaptchaUrlPost = value;
                OnPropertyChanged();
            }
        }

        private string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/80.0.3987.149 Safari/537.36";
        [Text("User-Agent:")]
        public string UserAgent
        {
            get { return userAgent; }
            set
            {
                userAgent = value;
                OnPropertyChanged();
            }
        }

        public override void Process(BotData data)
        {
            //GET
            var blockRequest = new BlockRequest();
            blockRequest.Url = recaptchaUrlGet;
            blockRequest.SetCustomHeaders(new[] {
                $"UserAgent: {UserAgent}",
                "Accept: text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8",
                "Accept-Language: en-US,en;q=0.9",
                "Accept-Encoding: gzip, deflate",
                "Upgrade-Insecure-Requests: 1",
                "Connection: keep-alive",
            });
            blockRequest.Process(data);

            var recaptchaToken = Regex.Match(ReplaceValues("<SOURCE>", data), "id=\"recaptcha-token\" value=\"(.*?)\">").Groups[1].Value;

            //POST
            blockRequest.PostData = $"v={Regex.Match(blockRequest.Url, "v=(.*?)&").Groups[1].Value}&reason=q&c={recaptchaToken}&k={Regex.Match(blockRequest.Url, "&k=(.*?)&").Groups[1].Value}&co={Regex.Match(blockRequest.Url, "&co=(.*?)&").Groups[1].Value}&hl=en&size=invisible&chr=%5B89%2C64%2C27%5D&vh=13599012192&bg={bg}";
            blockRequest.SetCustomHeaders(new[] {
                $"UserAgent: {UserAgent}",
                "Accept: */*",
                "Accept-encoding: gzip, deflate, br",
                "accept-language: fa,en;q=0.9,en-GB;q=0.8,en-US;q=0.7",
                $"Content-Length: {blockRequest.PostData.Length}",
                "Connection: keep-alive",
                "origin: https://www.google.com",
                $"referer: {blockRequest.Url}",
                "sec-fetch-dest: empty",
                "sec-fetch-mode: cors",
                "sec-fetch-site: same-origin",
            });
            blockRequest.Url = recaptchaUrlPost;
            blockRequest.Method = HttpMethod.POST;
            blockRequest.Process(data);

            var rresp = Regex.Match(ReplaceValues("<SOURCE>", data), "\"rresp\",\"(.*?)\"").Groups[1].Value;

            InsertVariable(data, false, rresp, VariableName);
        }

        public override string ToLS(bool indent = true)
        {
            var writer = new BlockWriter(GetType(), indent, Disabled);

            writer.Label(Label)
                .Token(nameof(RecaptchaV3Bypass))
                .Literal(RecaptchaUrlGet)
                .Literal(BG)
                .Literal(RecaptchaUrlPost);

            if (!writer.CheckDefault(VariableName, nameof(VariableName)))
            {
                writer.Arrow()
                    .Token("VAR")
                    .Literal(VariableName)
                    .Indent();
            }

            return writer.ToString();
        }

        public override BlockBase FromLS(string line)
        {
            // Trim the line
            var input = line.Trim();

            // Parse the label
            if (input.StartsWith("#"))
                Label = LineParser.ParseLabel(ref input);

            RecaptchaUrlGet = LineParser.ParseLiteral(ref input, nameof(RecaptchaUrlGet));
            BG = LineParser.ParseLiteral(ref input, nameof(BG));
            RecaptchaUrlPost = LineParser.ParseLiteral(ref input, nameof(RecaptchaUrlPost));

            if (LineParser.ParseToken(ref input, TokenType.Arrow, false) == "")
                return this;

            // Parse the variable/capture name
            try { VariableName = LineParser.ParseToken(ref input, TokenType.Literal, true); }
            catch { throw new ArgumentException("Variable name not specified"); }

            return this;
        }

        private Color ColorConverter(string color)
        {
            return (Color)System.Windows.Media.ColorConverter.ConvertFromString(color);
        }
    }
}
