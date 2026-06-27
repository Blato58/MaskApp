package cn.com.heaton.shiningmask.base.app;

import com.alibaba.fastjson2.JSONB;

/* JADX INFO: loaded from: classes.dex */
public class Command {
    public static final int MODE_BLUE_FLASH = 15;
    public static final int MODE_BLUE_GRADUAL = 2;
    public static final int MODE_COLORFUL_FLASH = 12;
    public static final int MODE_COLORFUL_GRADUAL = 10;
    public static final int MODE_COLORFUL_SWITCH = 11;
    public static final int MODE_CYAN_FLASH = 17;
    public static final int MODE_CYAN_GRADUAL = 4;
    public static final int MODE_FLASH_START = 12;
    public static final int MODE_GRADUAL_START = 0;
    public static final int MODE_GREEN_BLUEG_RADUAL = 9;
    public static final int MODE_GREEN_FLASH = 14;
    public static final int MODE_GREEN_GRADUAL = 1;
    public static final int MODE_PURPLE_FLASH = 18;
    public static final int MODE_PURPLE_GRADUAL = 5;
    public static final int MODE_RED_BLIE_GRADUAL = 8;
    public static final int MODE_RED_FLASH = 13;
    public static final int MODE_RED_GRADUAL = 0;
    public static final int MODE_RED_GREEN_GRADUAL = 7;
    public static final int MODE_WHITE_FLASH = 19;
    public static final int MODE_WHITE_GRADUAL = 6;
    public static final int MODE_YELLOW_FLASH = 16;
    public static final int MODE_YELLOW_GRADUAL = 3;
    public static byte[] ComSyncColor = {67, 79, 76, 82};
    public static int ComSyncColorLen = 8;
    public static byte[] ComSyncMode = {JSONB.Constants.BC_STR_ASCII_FIX_4, 79, JSONB.Constants.BC_INT32_SHORT_ZERO, 69};
    public static int ComSyncModeLen = 6;
    public static byte[] ComSyncLevel = {76, 69, 86, 76};
    public static int ComSyncLevelLen = 6;
    public static byte[] ComSyncLight = {80, 79, 87, 82, 0};
    public static int ComSyncLightLen = 5;
    public static byte[] ComSyncCalling = {67, 65, 76, 76};
    public static int ComSyncCallingLen = 15;
}