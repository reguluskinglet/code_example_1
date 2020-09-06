using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using demo.Authorization.Contexts;
using demo.DDD;
using demo.Http.ServiceResponse;
using demo.MessageContracts.InboxDistribution;
using demo.MessageContracts.InboxDistribution.Enums;
using demo.DemoApi.DAL.Migrations;
using demo.DemoApi.DAL.Repositories;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Service.ApplicationServices;
using demo.DemoApi.Service.Controllers;
using demo.DemoApi.Service.Dtos;
using demo.Transit.Publisher;
using Sms = demo.MessageContracts.Dtos.Sms.Sms;

namespace demo.DemoApi.Service.Tests.Core
{
    public static class TestExtension
    {
        private const string DefaultExtension = "700";

        public static async Task<IncomingInboxIntegrationEvent> MakeCall(
            this ServiceProvider serviceProvider,
            string callerExtension = DefaultExtension)
        {
            var newCall = await MakeCommonCall(serviceProvider, callerExtension);
            return newCall;
        }

        public static async Task<IncomingInboxIntegrationEvent> MakeSms(
            this ServiceProvider serviceProvider,
            string smsText,
            string callerExtension = DefaultExtension)
        {
            var newCall = await MakeCommonCall(serviceProvider, callerExtension, smsText);
            return newCall;
        }

        private static async Task<IncomingInboxIntegrationEvent> MakeCommonCall(
            this ServiceProvider serviceProvider,
            string callerExtension,
            string smsText = null)
        {
            var callManagementService = serviceProvider.GetScopedService<CallManagementService>();

            var newCall = new IncomingInboxIntegrationEvent
            {
                ItemId = Guid.NewGuid(),
                CallerExtension = callerExtension,
                Sms = new Sms
                {
                    Message = smsText
                },
                ContractInboxItemType = smsText == null ? ContractInboxItemType.Call : ContractInboxItemType.Sms
            };
            await callManagementService.AddIncomingCall(newCall);
            return newCall;
        }

        public static T GetScopedService<T>(this ServiceProvider serviceProvider)
        {
            return serviceProvider.CreateScope().ServiceProvider.GetService<T>();
        }

        public static T GetScopedController<T>(this ServiceProvider serviceProvider, Guid userId)
        {
            var scopedProvider = serviceProvider.CreateScope().ServiceProvider;
            AuthorizationContext.CurrentUserId = userId;
            return scopedProvider.GetService<T>();
        }
    }
}
