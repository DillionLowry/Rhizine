﻿namespace WPFtemplate.Core.Contracts.Services;

public interface IIdentityCacheService
{
    void SaveMsalToken(byte[] token);

    byte[] ReadMsalToken();
}