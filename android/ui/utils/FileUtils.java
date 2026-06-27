package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;
import android.os.Environment;
import android.util.Log;
import java.io.BufferedReader;
import java.io.File;
import java.io.FileInputStream;
import java.io.FileNotFoundException;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.io.InputStreamReader;

/* JADX INFO: loaded from: classes.dex */
public class FileUtils {
    public static void copyBigDataToSD(Context context, String str, String str2) throws IOException {
        FileOutputStream fileOutputStream = new FileOutputStream(str2);
        InputStream inputStreamOpen = context.getAssets().open(str);
        byte[] bArr = new byte[1024];
        for (int i = inputStreamOpen.read(bArr); i > 0; i = inputStreamOpen.read(bArr)) {
            fileOutputStream.write(bArr, 0, i);
        }
        Log.e("FlieUtils==", "复制到sd卡完成");
        fileOutputStream.flush();
        inputStreamOpen.close();
        fileOutputStream.close();
    }

    public static String getFileTxtContent(File file) {
        String str = "";
        if (!file.isDirectory() && file.getName().endsWith("txt")) {
            try {
                FileInputStream fileInputStream = new FileInputStream(file);
                BufferedReader bufferedReader = new BufferedReader(new InputStreamReader(fileInputStream, "UTF-8"));
                while (true) {
                    String line = bufferedReader.readLine();
                    if (line != null) {
                        str = str + line + "\n";
                    } else {
                        fileInputStream.close();
                        return str;
                    }
                }
            } catch (FileNotFoundException unused) {
                Log.d("TestFile", "The File doesn't not exist.");
            } catch (IOException e) {
                Log.d("TestFile", e.getMessage());
            }
        }
        return str;
    }

    public static File getExternalFilePath(Context context, String str) {
        if ("mounted".equals(Environment.getExternalStorageState()) || !Environment.isExternalStorageRemovable()) {
            return context.getExternalFilesDir(str);
        }
        File file = new File(context.getFilesDir() + File.separator + str);
        if (!file.exists()) {
            file.mkdirs();
        }
        return file;
    }
}