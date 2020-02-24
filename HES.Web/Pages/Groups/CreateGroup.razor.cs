﻿using HES.Core.Entities;
using HES.Core.Enums;
using HES.Core.Interfaces;
using HES.Web.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace HES.Web.Pages.Groups
{
    public partial class CreateGroup : ComponentBase
    {
        [Inject] public IGroupService GroupService { get; set; }
        [Inject] public ILogger<CreateGroup> Logger { get; set; }
        [Parameter] public EventCallback Refresh { get; set; }

        public Group Group = new Group();

        private async Task CreateAsync()
        {
            try
            {
                await GroupService.CreateGroupAsync(Group);
                await Refresh.InvokeAsync(this);
                ToastService.ShowToast("Group created.", ToastLevel.Success);
            }
            catch (Exception ex)
            {
                Logger.LogError(ex.Message);
                ToastService.ShowToast(ex.Message, ToastLevel.Error);
            }
            finally
            {
                await MainWrapper.ModalDialogComponent.CloseAsync();
            }
        }
    }
}