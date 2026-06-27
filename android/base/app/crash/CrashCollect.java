package cn.com.heaton.shiningmask.base.app.crash;

import android.content.Context;
import android.content.pm.PackageInfo;
import android.content.pm.PackageManager;
import android.os.Build;
import android.os.Process;
import android.provider.Settings;
import cn.com.heaton.shiningmask.ui.utils.AppUtils;
import cn.com.heaton.shiningmask.ui.utils.FileUtils;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import com.cdbwsoft.library.AppConfig;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStreamReader;
import java.io.PrintWriter;
import java.io.StringWriter;
import java.lang.reflect.Field;
import java.text.DateFormat;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Map;
import java.util.concurrent.ConcurrentHashMap;

/* JADX INFO: loaded from: classes.dex */
public class CrashCollect {
    public static final String BUILD_INFOS_MAP = "BUILD_INFOS_MAP";
    public static final String CRASH_LOG_NAME = "crash_log.txt";
    public static final String EXCEPETION_INFOS_STRING = "EXCEPETION_INFOS_STRING";
    public static final String MEMORY_INFOS_STRING = "MEMORY_INFOS_STRING";
    public static final String PACKAGE_INFOS_MAP = "PACKAGE_INFOS_MAP";
    public static final String SECURE_INFOS_MAP = "SECURE_INFOS_MAP";
    public static final String SYSTEM_INFOS_MAP = "SYSTEM_INFOS_MAP";
    public static final String VERSION_CODE = "versionCode";
    public static final String VERSION_NAME = "versionName";
    private static File crashFile;
    private Context context;
    private String exceptionInfos;
    private ConcurrentHashMap<String, Object> infos = new ConcurrentHashMap<>();
    private ConcurrentHashMap<String, String> packageInfos = new ConcurrentHashMap<>();
    private ConcurrentHashMap<String, String> deviceInfos = new ConcurrentHashMap<>();
    private ConcurrentHashMap<String, String> systemInfos = new ConcurrentHashMap<>();
    private ConcurrentHashMap<String, String> secureInfos = new ConcurrentHashMap<>();
    private DateFormat formatter = new SimpleDateFormat("yyyy-MM-dd-HH-mm-ss");

    CrashCollect(Context context) {
        this.context = context;
        crashFile = FileUtils.getExternalFilePath(context, "log");
    }

    public static File getCrashLogFile() {
        return new File(crashFile, CRASH_LOG_NAME);
    }

    CrashInfo collectCrashInfo(Throwable th) {
        LogUtil.d("collectCrashInfo:" + th);
        CrashInfo crashInfo = new CrashInfo();
        crashInfo.setAppPackage(AppUtils.getPackageName(this.context));
        crashInfo.setAppChannel(AppUtils.getAppMetaData(this.context, AppConfig.META_CHANNEL));
        crashInfo.setPhoneSystem(AppConfig.PLATFORM);
        crashInfo.setPhoneBrands(AppUtils.getDeviceBrand());
        crashInfo.setPhoneModel(AppUtils.getSystemModel());
        crashInfo.setPhoneSystemVersion(AppUtils.getSystemVersion());
        crashInfo.setAppVersionName(AppUtils.getVersionName(this.context));
        crashInfo.setAppVersionCode(String.valueOf(AppUtils.getVersionCode(this.context)));
        if (th != null) {
            crashInfo.setExceptionInfo(collectExceptionInfos(th));
            return crashInfo;
        }
        File crashLogFile = getCrashLogFile();
        if (crashLogFile.exists()) {
            crashInfo.setExceptionInfo(FileUtils.getFileTxtContent(crashLogFile));
        }
        return crashInfo;
    }

    void saveCrashInfoToFile(String str) {
        LogUtil.d("保存文件：" + str);
        try {
            FileOutputStream fileOutputStream = new FileOutputStream(new File(crashFile, CRASH_LOG_NAME));
            fileOutputStream.write(str.getBytes());
            fileOutputStream.close();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e2) {
            e2.printStackTrace();
        }
    }

    public void collectInfos(Context context, Throwable th) throws Throwable {
        this.exceptionInfos = collectExceptionInfos(th);
        collectPackageInfos(context);
        collectBuildInfos();
        collectSystemInfos();
        collectSecureInfos();
        String strCollectMemInfos = collectMemInfos();
        this.infos.put(EXCEPETION_INFOS_STRING, this.exceptionInfos);
        this.infos.put(PACKAGE_INFOS_MAP, this.packageInfos);
        this.infos.put(BUILD_INFOS_MAP, this.deviceInfos);
        this.infos.put(SYSTEM_INFOS_MAP, this.systemInfos);
        this.infos.put(SECURE_INFOS_MAP, this.secureInfos);
        this.infos.put(MEMORY_INFOS_STRING, strCollectMemInfos);
    }

    private void saveCrashInfo2File() {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < this.infos.size(); i++) {
            sb.append(this.infos.values());
        }
        try {
            FileOutputStream fileOutputStream = new FileOutputStream(new File(crashFile, CRASH_LOG_NAME));
            fileOutputStream.write(sb.toString().getBytes());
            fileOutputStream.close();
        } catch (FileNotFoundException e) {
            e.printStackTrace();
        } catch (IOException e2) {
            e2.printStackTrace();
        }
    }

    private String collectExceptionInfos(Throwable th) {
        LogUtil.d("获取捕获异常的信息:" + th);
        StringWriter stringWriter = new StringWriter();
        PrintWriter printWriter = new PrintWriter(stringWriter);
        th.printStackTrace(printWriter);
        th.printStackTrace();
        for (Throwable cause = th.getCause(); cause != null; cause = cause.getCause()) {
            cause.printStackTrace(printWriter);
            printWriter.append("\r\n");
        }
        printWriter.close();
        LogUtil.d("writer:" + stringWriter);
        return stringWriter.toString();
    }

    private String collectMemInfos() throws Throwable {
        StringBuffer stringBuffer = new StringBuffer();
        ArrayList arrayList = new ArrayList();
        arrayList.add("dumpsys");
        arrayList.add("meminfo");
        arrayList.add(Integer.toString(Process.myPid()));
        BufferedReader bufferedReader = null;
        try {
            try {
                try {
                    BufferedReader bufferedReader2 = new BufferedReader(new InputStreamReader(Runtime.getRuntime().exec((String[]) arrayList.toArray(new String[arrayList.size()])).getInputStream()), 8192);
                    while (true) {
                        try {
                            String line = bufferedReader2.readLine();
                            if (line == null) {
                                break;
                            }
                            stringBuffer.append(line);
                            stringBuffer.append("\n");
                        } catch (IOException e) {
                            e = e;
                            bufferedReader = bufferedReader2;
                            e.printStackTrace();
                            if (bufferedReader != null) {
                                bufferedReader.close();
                            }
                            return stringBuffer.toString();
                        } catch (Throwable th) {
                            th = th;
                            bufferedReader = bufferedReader2;
                            if (bufferedReader != null) {
                                try {
                                    bufferedReader.close();
                                } catch (IOException e2) {
                                    e2.printStackTrace();
                                }
                            }
                            throw th;
                        }
                    }
                    bufferedReader2.close();
                } catch (Throwable th2) {
                    th = th2;
                }
            } catch (IOException e3) {
                e = e3;
            }
        } catch (IOException e4) {
            e4.printStackTrace();
        }
        return stringBuffer.toString();
    }

    private void collectSecureInfos() {
        for (Field field : Settings.Secure.class.getFields()) {
            if (!field.isAnnotationPresent(Deprecated.class) && field.getType() == String.class && field.getName().startsWith("WIFI_AP")) {
                try {
                    String string = Settings.Secure.getString(this.context.getContentResolver(), (String) field.get(null));
                    if (string != null) {
                        this.secureInfos.put(field.getName(), string);
                    }
                } catch (IllegalAccessException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    private void collectSystemInfos() {
        for (Field field : Settings.System.class.getFields()) {
            if (!field.isAnnotationPresent(Deprecated.class) && field.getType() == String.class) {
                try {
                    String string = Settings.System.getString(this.context.getContentResolver(), (String) field.get(null));
                    if (string != null) {
                        this.systemInfos.put(field.getName(), string);
                    }
                } catch (IllegalAccessException e) {
                    e.printStackTrace();
                }
            }
        }
    }

    private void collectBuildInfos() {
        for (Field field : Build.class.getDeclaredFields()) {
            try {
                field.setAccessible(true);
                this.deviceInfos.put(field.getName(), field.get("").toString());
            } catch (IllegalAccessException e) {
                e.printStackTrace();
            } catch (IllegalArgumentException e2) {
                e2.printStackTrace();
            }
        }
    }

    private void collectPackageInfos(Context context) {
        try {
            PackageInfo packageInfo = context.getPackageManager().getPackageInfo(context.getPackageName(), 1);
            if (packageInfo != null) {
                String str = packageInfo.versionName == null ? "null" : packageInfo.versionName;
                String str2 = packageInfo.versionCode + "";
                this.packageInfos.put(VERSION_NAME, str);
                this.packageInfos.put(VERSION_CODE, str2);
            }
        } catch (PackageManager.NameNotFoundException e) {
            e.printStackTrace();
        }
    }

    public static StringBuffer getInfoStr(ConcurrentHashMap<String, String> concurrentHashMap) {
        StringBuffer stringBuffer = new StringBuffer();
        for (Map.Entry<String, String> entry : concurrentHashMap.entrySet()) {
            stringBuffer.append(entry.getKey() + "=" + entry.getValue() + "\r\n");
        }
        return stringBuffer;
    }
}