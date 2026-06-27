package cn.com.heaton.shiningmask.base.update;

import android.app.Activity;
import android.content.DialogInterface;
import android.content.Intent;
import android.os.Handler;
import android.os.Message;
import android.text.TextUtils;
import android.util.Log;
import android.view.View;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;
import androidx.appcompat.app.AlertDialog;
import androidx.core.app.NotificationCompat;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.utils.AppUtils;
import cn.com.heaton.shiningmask.ui.utils.FileProvider7;
import cn.com.heaton.shiningmask.ui.utils.FileUtils;
import cn.com.heaton.shiningmask.ui.utils.LogInterceptor;
import com.alibaba.fastjson.JSON;
import com.alibaba.fastjson.JSONException;
import com.alibaba.fastjson.JSONObject;
import com.cdbwsoft.library.AppConfig;
import com.cdbwsoft.library.vo.UpdateVO;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import okhttp3.Call;
import okhttp3.Callback;
import okhttp3.FormBody;
import okhttp3.OkHttpClient;
import okhttp3.Request;
import okhttp3.Response;

/* JADX INFO: loaded from: classes.dex */
public class UpdateManager {
    private static final int DOWN_BEFORE = 4;
    private static final int DOWN_FAIL = 3;
    private static final int DOWN_OVER = 2;
    private static final int DOWN_UPDATE = 1;
    private static final String TAG = "UpdateManager";
    public static boolean isNeedUpDateApp = false;
    private Activity mContext;
    private AlertDialog mDialog;
    private AlertDialog mDownloadDialog;
    private DownloadListener mDownloadListener;
    private DownloadThread mDownloadThread;
    private ProgressBar mProgress;
    private boolean mShowDialog;
    private float mSize;
    private TextView mTvTotal;
    private boolean interceptFlag = false;
    private boolean mApk = true;
    private MessageHandler mHandler = new MessageHandler();

    public interface DownloadListener {
        void onDownloadComplete();

        void onDownloadFailed();

        void onDownloading(int i);

        void onInstall(File file);

        void onPreDownload(String str);
    }

    private class MessageHandler extends Handler {
        private MessageHandler() {
        }

        @Override // android.os.Handler
        public void dispatchMessage(Message message) {
            int i = message.what;
            if (i == 1) {
                if (UpdateManager.this.mProgress != null) {
                    UpdateManager.this.mProgress.setProgress(message.arg1);
                }
                if (UpdateManager.this.mDownloadListener != null) {
                    UpdateManager.this.mDownloadListener.onDownloading(message.arg1);
                    return;
                }
                return;
            }
            if (i == 2) {
                if (UpdateManager.this.mDownloadListener != null) {
                    UpdateManager.this.mDownloadListener.onDownloadComplete();
                }
                UpdateManager.this.install((File) message.obj);
                return;
            }
            if (i != 3) {
                if (i == 4) {
                    if (UpdateManager.this.mDownloadListener != null) {
                        UpdateManager.this.mDownloadListener.onPreDownload((String) message.obj);
                        return;
                    }
                    return;
                }
                super.dispatchMessage(message);
                return;
            }
            if (UpdateManager.this.mDownloadDialog != null) {
                UpdateManager.this.mDownloadDialog.dismiss();
                AlertDialog.Builder builder = new AlertDialog.Builder(UpdateManager.this.mContext);
                builder.setCancelable(false);
                builder.setMessage(R.string.update_fail);
                final String str = (String) message.obj;
                final float f = UpdateManager.this.mSize;
                builder.setPositiveButton(R.string.read_write_tip, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.MessageHandler.1
                    @Override // android.content.DialogInterface.OnClickListener
                    public void onClick(DialogInterface dialogInterface, int i2) {
                        dialogInterface.dismiss();
                        UpdateManager.this.downloadFile(str, f, UpdateManager.this.mShowDialog);
                    }
                });
                builder.setNegativeButton(R.string.btn_cancel, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.MessageHandler.2
                    @Override // android.content.DialogInterface.OnClickListener
                    public void onClick(DialogInterface dialogInterface, int i2) {
                        dialogInterface.dismiss();
                    }
                });
                builder.show();
            }
            if (UpdateManager.this.mDownloadListener != null) {
                UpdateManager.this.mDownloadListener.onDownloadFailed();
            }
        }
    }

    public UpdateManager(Activity activity) {
        this.mContext = activity;
    }

    public void versionUpdate() {
        OkHttpClient okHttpClientBuild = new OkHttpClient.Builder().addInterceptor(new LogInterceptor()).build();
        FormBody.Builder builder = new FormBody.Builder();
        builder.add("app_id", AppUtils.getPackageName(this.mContext));
        builder.add("platform", AppConfig.PLATFORM);
        okHttpClientBuild.newCall(new Request.Builder().url("http://api.e-toys.cn/api/app/lastUpdate").post(builder.build()).build()).enqueue(new Callback() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.1
            @Override // okhttp3.Callback
            public void onFailure(Call call, IOException iOException) {
            }

            @Override // okhttp3.Callback
            public void onResponse(Call call, Response response) throws IOException {
                final UpdateVO updateVO;
                try {
                    JSONObject object = JSON.parseObject(response.body().string());
                    if (object.getInteger(NotificationCompat.CATEGORY_STATUS).intValue() == 0) {
                        String string = object.getString("data");
                        if (TextUtils.isEmpty(string) || (updateVO = (UpdateVO) JSONObject.parseObject(string, UpdateVO.class)) == null) {
                            return;
                        }
                        int versionCode = AppUtils.getVersionCode(UpdateManager.this.mContext);
                        if (!TextUtils.isEmpty(updateVO.app_url) && versionCode >= 0 && updateVO.app_version_number > versionCode) {
                            Log.i(UpdateManager.TAG, "onResponse: 检测到新版本");
                            UpdateManager.isNeedUpDateApp = true;
                            final String str = UpdateManager.this.mContext.getResources().getString(R.string.find_new_version) + ":" + updateVO.app_version + "\n\n" + UpdateManager.this.mContext.getResources().getString(R.string.size) + ":" + (((int) ((updateVO.app_size / 1024.0f) * 100.0f)) / 100) + "MB\n\n" + updateVO.app_update;
                            UpdateManager.this.mHandler.post(new Runnable() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.1.1
                                @Override // java.lang.Runnable
                                public void run() {
                                    UpdateManager.this.showNoticeDialog(str, updateVO.app_url, updateVO.app_size);
                                }
                            });
                            return;
                        }
                        UpdateManager.isNeedUpDateApp = false;
                    }
                } catch (JSONException e) {
                    e.printStackTrace();
                }
            }
        });
    }

    public void showNoticeDialog(String str, String str2, float f) {
        showNoticeDialog(str, str2, f, true);
    }

    public void showNoticeDialog(String str, final String str2, final float f, final boolean z) {
        AlertDialog.Builder builder = new AlertDialog.Builder(this.mContext);
        builder.setCancelable(false);
        builder.setTitle(com.cdbwsoft.library.R.string.update_title);
        builder.setMessage(str);
        builder.setPositiveButton(com.cdbwsoft.library.R.string.btn_sure, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.2
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
                UpdateManager.this.downloadFile(str2, f, z);
            }
        });
        builder.setNegativeButton(com.cdbwsoft.library.R.string.update_after, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.3
            @Override // android.content.DialogInterface.OnClickListener
            public void onClick(DialogInterface dialogInterface, int i) {
                dialogInterface.dismiss();
            }
        });
        this.mDialog = builder.show();
    }

    public void setDownloadDialog(AlertDialog alertDialog) {
        if (alertDialog != this.mDownloadDialog) {
            this.mDownloadDialog = alertDialog;
        }
    }

    public void downloadFile(String str, float f, boolean z) {
        if (TextUtils.isEmpty(str)) {
            Toast.makeText(this.mContext, com.cdbwsoft.library.R.string.download_url_invalid, 1).show();
            return;
        }
        File externalFilePath = FileUtils.getExternalFilePath(this.mContext, ".apk");
        Log.e("TAG", "UpdateManager>>>[downloadFile]: " + externalFilePath);
        if (externalFilePath.exists() || !externalFilePath.mkdir()) {
            String strSubstring = str.substring(str.lastIndexOf("/"));
            File file = new File(externalFilePath, strSubstring);
            if (file.exists()) {
                install(file);
                return;
            }
            try {
                if (this.mDownloadDialog == null) {
                    AlertDialog.Builder builder = new AlertDialog.Builder(this.mContext);
                    builder.setCancelable(false);
                    builder.setTitle(com.cdbwsoft.library.R.string.update_title);
                    builder.setView(com.cdbwsoft.library.R.layout.update_layout);
                    this.mDownloadDialog = builder.create();
                    builder.setNegativeButton(R.string.btn_cancel, new DialogInterface.OnClickListener() { // from class: cn.com.heaton.shiningmask.base.update.UpdateManager.4
                        @Override // android.content.DialogInterface.OnClickListener
                        public void onClick(DialogInterface dialogInterface, int i) {
                            dialogInterface.dismiss();
                            UpdateManager.this.mDownloadThread.interrupt();
                            UpdateManager.this.interceptFlag = true;
                        }
                    });
                }
                this.mSize = f;
                this.mDownloadDialog.show();
                View viewFindViewById = this.mDownloadDialog.findViewById(com.cdbwsoft.library.R.id.progress);
                if (viewFindViewById instanceof ProgressBar) {
                    this.mProgress = (ProgressBar) viewFindViewById;
                }
                View viewFindViewById2 = this.mDownloadDialog.findViewById(com.cdbwsoft.library.R.id.tv_total);
                if (viewFindViewById2 instanceof TextView) {
                    TextView textView = (TextView) viewFindViewById2;
                    this.mTvTotal = textView;
                    textView.setText(this.mContext.getString(com.cdbwsoft.library.R.string.total_size, new Object[]{Float.valueOf(this.mSize / 1024.0f)}));
                }
                this.mShowDialog = z;
                downloadApk(new File(externalFilePath, strSubstring + ".tmp"), str);
            } catch (Exception e) {
                e.printStackTrace();
            }
        }
    }

    public void showDownloadDialog(String str, float f) {
        downloadFile(str, f, true);
    }

    private class DownloadThread extends Thread {
        String downloadUrl;
        File saveFile;

        DownloadThread(File file, String str) {
            this.saveFile = file;
            this.downloadUrl = str;
        }

        @Override // java.lang.Thread, java.lang.Runnable
        public void run() {
            try {
                HttpURLConnection httpURLConnection = (HttpURLConnection) new URL(this.downloadUrl).openConnection();
                httpURLConnection.connect();
                int contentLength = httpURLConnection.getContentLength();
                InputStream inputStream = httpURLConnection.getInputStream();
                FileOutputStream fileOutputStream = new FileOutputStream(this.saveFile);
                UpdateManager.this.mHandler.obtainMessage(4, this.downloadUrl).sendToTarget();
                byte[] bArr = new byte[1024];
                int i = 0;
                while (true) {
                    int i2 = inputStream.read(bArr);
                    i += i2;
                    UpdateManager.this.mHandler.obtainMessage(1, (int) ((i / contentLength) * 100.0f), 0).sendToTarget();
                    if (i2 <= 0) {
                        File file = new File(this.saveFile.getCanonicalPath().replace(".tmp", ""));
                        this.saveFile.renameTo(file);
                        UpdateManager.this.mHandler.obtainMessage(2, file).sendToTarget();
                        break;
                    } else {
                        fileOutputStream.write(bArr, 0, i2);
                        if (UpdateManager.this.interceptFlag) {
                            break;
                        }
                    }
                }
                fileOutputStream.close();
                inputStream.close();
            } catch (IOException e) {
                e.printStackTrace();
                UpdateManager.this.mHandler.obtainMessage(3, this.downloadUrl).sendToTarget();
            }
        }
    }

    public DownloadListener getDownloadListener() {
        return this.mDownloadListener;
    }

    public void setDownloadListener(DownloadListener downloadListener) {
        this.mDownloadListener = downloadListener;
    }

    public boolean isApk() {
        return this.mApk;
    }

    public void setApk(boolean z) {
        this.mApk = z;
    }

    private void downloadApk(File file, String str) {
        DownloadThread downloadThread = this.mDownloadThread;
        if (downloadThread == null || !downloadThread.isAlive()) {
            DownloadThread downloadThread2 = new DownloadThread(file, str);
            this.mDownloadThread = downloadThread2;
            downloadThread2.start();
        }
    }

    /* JADX INFO: Access modifiers changed from: private */
    public void install(File file) {
        if (file.exists()) {
            AlertDialog alertDialog = this.mDownloadDialog;
            if (alertDialog != null) {
                alertDialog.dismiss();
            }
            DownloadListener downloadListener = this.mDownloadListener;
            if (downloadListener != null) {
                downloadListener.onInstall(file);
            }
            if (this.mApk) {
                Intent intent = new Intent("android.intent.action.VIEW");
                intent.setFlags(268435456);
                FileProvider7.setIntentDataAndType(this.mContext, intent, "application/vnd.android.package-archive", file, true);
                this.mContext.startActivity(intent);
            }
        }
    }
}