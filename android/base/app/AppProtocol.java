package cn.com.heaton.shiningmask.base.app;

import com.alibaba.fastjson2.JSONB;

/* JADX INFO: loaded from: classes.dex */
public class AppProtocol {
    public static final int MODEL_RGB = 27;
    private static final String TAG = "AppProtocol";
    public static byte[] SwitchLightCommand = {80, 79, 87, 82, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] TimeCommand = {84, 73, JSONB.Constants.BC_STR_ASCII_FIX_4, 69, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] TimerONCommand = {84, JSONB.Constants.BC_STR_ASCII_FIX_4, 69, 82, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] ModeCommand = {80, 79, 87, 82, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] PwdCommand = {80, 73, JSONB.Constants.BC_STR_ASCII_FIX_5, 88, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] lightTypeCommand = {76, 73, JSONB.Constants.BC_INT32_SHORT_MAX, 84, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] SpeedCommand = {83, 80, 69, JSONB.Constants.BC_INT32_SHORT_ZERO, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
    public static byte[] RhyhmnCommand = {76, 69, 86, 76, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
}