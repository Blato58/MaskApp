package cn.com.heaton.shiningmask.base.app;

import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;

/* JADX INFO: loaded from: classes.dex */
public class C {

    @Retention(RetentionPolicy.SOURCE)
    public @interface API {
        public static final String APP_LAST_UPDATE = "app/lastUpdate";
        public static final String APP_UPLOAD_CRASH = "App/add_app_crash";
        public static final String APP_UPLOAD_INSTALL = "app/count";
        public static final String BASE_URL = "http://api.e-toys.cn/api/";
    }

    @Retention(RetentionPolicy.SOURCE)
    public @interface Constance {
        public static final String APP_HOST = "http://api.icworkshop.com/shop/";
        public static final String BLE_AVERTISE_NAME = "BTCar-";
        public static final int BLUE_CAR = 1;
        public static final String CAR_TYPE = "car_type";
        public static final int CONTROL_FLAG = 1;
        public static final int DIY_IMAGE_FRAMGMENT_FLAG = 1;
        public static final int GRAVITY_FLAG = 0;
        public static final int GREEN_CAR = 2;
        public static final String MODE = "mode";
        public static final int RED_CAR = 0;
        public static final String SCENES = "scenes";
        public static final int VOICE_FLAG = 2;
    }

    @Retention(RetentionPolicy.SOURCE)
    public @interface LANGUAGE {
        public static final int ID_DE = 4;
        public static final int ID_EN = 1;
        public static final int ID_FR = 3;
        public static final int ID_PT = 2;
        public static final int ID_ZN = 0;
    }

    @Retention(RetentionPolicy.SOURCE)
    public @interface MAIN_EVENT {
        public static final String ABNORMALLY_DISCONNECT = "abnormally_disconnect";
        public static final String BIND_FFT = "bind_fft";
        public static final String CONNECTED = "connected";
        public static final String FINISH_CAMERAACTIVITY = "finish_cameraactivity";
        public static final String PLAYER_FRAGMENT = "player_fragment";
        public static final String PLAY_STATE_CHANGED = "play_state_chaned";
        public static final String RHYTHM_FRAGMENT = "rhythm_fragment";
        public static final String SEARCH_FRAGMENT = "search_fragment";
        public static final String SHOW_DIY_BTN = "show_diy_btn";
        public static final String SHUTDOWN = "shutdown";
        public static final String START_RHY = "start_rhy";
        public static final String STOP_RHY = "stop_rhy";
        public static final String STOP_RHY1 = "stop_rhy1";
        public static final String UPDATE_ANIM = "update_anim";
        public static final String UPDATE_DIY_IMAGE_LIST = "UPDATE_DIY_IMAGE_LIST";
        public static final String UPDATE_DIY_LIST = "update_diy_list";
    }

    @Retention(RetentionPolicy.SOURCE)
    public @interface SET {
        public static final int ID_ABOUT = 1;
        public static final int ID_LANGUAGE = 0;
    }

    public interface SP {
        public static final String BLE_AVERTISE_DEVICE = "ble_avertise_device";
        public static final int CONNECT_STATUS_CONNNECTED = 1;
        public static final int CONNECT_STATUS_DISCONNECT = 0;
        public static final String FIRST_INSTALL = "first_install";
        public static final String LANGUAGE = "language";
        public static final String LANGUAGE_CHINESE_SIMPLIFIED = "zh";
        public static final String LANGUAGE_CHINESE_TRADITIONAL = "zh_TW";
        public static final String LANGUAGE_DE = "de";
        public static final String LANGUAGE_ENGLISH = "en";
        public static final String LANGUAGE_ES = "es";
        public static final String LANGUAGE_FR = "fr";
        public static final String LANGUAGE_HK = "zh_HK";
        public static final String LANGUAGE_JA = "ja";
        public static final String LANGUAGE_KO = "ko";
        public static final String LANGUAGE_PT = "pt";
        public static final String LANGUAGE_RU = "ru";
        public static final String RANDOM_VALUE = "random_value";
        public static final String ROLLING_CODE = "rolling_code";
        public static final String SETTINGS_DIY_IMAGE_DATA = "settings_diy_image_data";
        public static final String SETTINGS_GESTURE_ENABLE = "gesture_enable";
        public static final String SETTINGS_IMAGE_DATA = "settings_image_data";
        public static final String SETTINGS_IMAGE_SELECT_MODE = "image_select_mode";
        public static final String SETTINGS_LOOP_MODE = "loop_mode";
        public static final String TIMER01_HOUR_OFF = "timer01_hour_off";
        public static final String TIMER01_HOUR_ON = "timer01_hour_on";
        public static final String TIMER01_MIN_OFF = "timer01_min_off";
        public static final String TIMER01_MIN_ON = "timer01_min_on";
        public static final String TIMER01_STATUS = "timer01_status";
        public static final String TIMER02_HOUR_OFF = "timer02_hour_off";
        public static final String TIMER02_HOUR_ON = "timer02_hour_on";
        public static final String TIMER02_MIN_OFF = "timer02_min_off";
        public static final String TIMER02_MIN_ON = "timer02_min_on";
        public static final String TIMER02_STATUS = "timer02_status";
    }
}