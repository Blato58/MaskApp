package cn.com.heaton.shiningmask.base.app;

import android.content.Context;
import android.util.Base64;
import android.util.Log;
import androidx.core.app.NotificationCompat;
import cn.com.heaton.shiningmask.base.app.C;
import cn.com.heaton.shiningmask.base.app.crash.CrashCollect;
import cn.com.heaton.shiningmask.base.app.crash.CrashInfo;
import cn.com.heaton.shiningmask.ui.utils.AppUtils;
import cn.com.heaton.shiningmask.ui.utils.LogInterceptor;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.utils.SPUtils;
import cn.com.heaton.shiningmask.ui.utils.TimeUtils;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONException;
import com.cdbwsoft.library.AppConfig;
import java.io.File;
import java.io.IOException;
import java.nio.charset.StandardCharsets;
import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.FormBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/* JADX INFO: loaded from: classes.dex */
public class UploadManager {
    private static final String TAG = "UploadManager";

    public static void uploadInstallInfo(final Context context) {
        if (((Boolean) SPUtils.get(context, C.SP.FIRST_INSTALL, true)).booleanValue()) {
            OkHttpClient okHttpClientBuild = new OkHttpClient.Builder().addInterceptor(new LogInterceptor()).build();
            FormBody.Builder builder = new FormBody.Builder();
            builder.add("app_package", AppUtils.getPackageName(context));
            builder.add("app_channel", AppUtils.getAppMetaData(context, AppConfig.META_CHANNEL));
            builder.add("phone_system", AppConfig.PLATFORM);
            builder.add("phone_brands", AppUtils.getDeviceBrand());
            builder.add("phone_model", AppUtils.getSystemModel());
            builder.add("phone_system_version", AppUtils.getSystemVersion());
            builder.add("run_time", TimeUtils.getNowTime());
            okHttpClientBuild.newCall(new Request.Builder().url("http://api.e-toys.cn/api/app/count").post(builder.build()).build()).enqueue(new Callback() { // from class: cn.com.heaton.shiningmask.base.app.UploadManager.1
                @Override // okhttp3.Callback
                public void onFailure(Call call, IOException iOException) {
                }

                @Override // okhttp3.Callback
                public void onResponse(Call call, Response response) throws IOException {
                    try {
                        if (JSON.parseObject(response.body().string()).getInteger(NotificationCompat.CATEGORY_STATUS).intValue() == 0) {
                            Log.i(UploadManager.TAG, "onResponse: 上传安装信息成功");
                            SPUtils.put(context, C.SP.FIRST_INSTALL, false);
                        } else {
                            Log.e(UploadManager.TAG, "onResponse: 上传安装信息失败");
                        }
                    } catch (JSONException e) {
                        e.printStackTrace();
                    }
                }
            });
        }
    }

    public static void uploadCrashInfo(CrashInfo crashInfo) {
        OkHttpClient okHttpClientBuild = new OkHttpClient.Builder().build();
        FormBody.Builder builder = new FormBody.Builder();
        builder.add("app_package", crashInfo.getAppPackage());
        builder.add("app_channel", crashInfo.getAppChannel());
        builder.add("phone_system", crashInfo.getPhoneSystem());
        builder.add("phone_brands", crashInfo.getPhoneBrands());
        builder.add("phone_model", crashInfo.getPhoneModel());
        builder.add("phone_system_version", crashInfo.getPhoneSystemVersion());
        builder.add("app_version_name", crashInfo.getAppVersionName());
        builder.add("app_version_code", crashInfo.getAppVersionCode());
        builder.add("exception_info", Base64.encodeToString(crashInfo.getExceptionInfo().getBytes(StandardCharsets.UTF_8), 0));
        okHttpClientBuild.newCall(new Request.Builder().url("http://api.e-toys.cn/api/App/add_app_crash").post(builder.build()).build()).enqueue(new Callback() { // from class: cn.com.heaton.shiningmask.base.app.UploadManager.2
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
                LogUtil.w("UploadManager>>>[onFailure]: 上传错误日志信息出错");
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                try {
                    if (JSON.parseObject(response.body().string()).getInteger(NotificationCompat.CATEGORY_STATUS).intValue() == 0) {
                        LogUtil.d("UploadManager>>>[onResponse]: 上传错误日志信息成功");
                        File crashLogFile = CrashCollect.getCrashLogFile();
                        if (crashLogFile.exists()) {
                            crashLogFile.delete();
                            return;
                        }
                        return;
                    }
                    LogUtil.e("UploadManager>>>[onResponse]: 上传错误日志信息失败");
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        });
    }
}