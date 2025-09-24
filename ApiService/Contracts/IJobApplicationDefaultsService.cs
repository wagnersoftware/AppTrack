﻿using AppTrack.Frontend.ApiService.Base;
using AppTrack.Frontend.Models;

namespace AppTrack.Frontend.ApiService.Contracts;

public interface IJobApplicationDefaultsService
{
    Task<JobApplicationDefaultsModel> GetForUserAsync(int userId);

    Task UpdateForUserAsync(int userId, JobApplicationDefaultsModel jobApplicationDefaultsModel);
}
