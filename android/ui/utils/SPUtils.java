package cn.com.heaton.shiningmask.ui.utils;

import android.content.Context;
import android.content.SharedPreferences;
import java.lang.reflect.Method;
import java.util.Map;

/* JADX INFO: loaded from: classes.dex */
public class SPUtils {
    public static final String FILE_NAME = "share_data";

    public static void put(Context context, String str, Object obj) {
        SharedPreferences.Editor editorEdit = context.getSharedPreferences(FILE_NAME, 0).edit();
        if (obj instanceof String) {
            editorEdit.putString(str, (String) obj);
        } else if (obj instanceof Integer) {
            editorEdit.putInt(str, ((Integer) obj).intValue());
        } else if (obj instanceof Boolean) {
            editorEdit.putBoolean(str, ((Boolean) obj).booleanValue());
        } else if (obj instanceof Float) {
            editorEdit.putFloat(str, ((Float) obj).floatValue());
        } else if (obj instanceof Long) {
            editorEdit.putLong(str, ((Long) obj).longValue());
        } else {
            editorEdit.putString(str, obj == null ? null : String.valueOf(obj));
        }
        SharedPreferencesCompat.apply(editorEdit);
    }

    /* JADX WARN: Multi-variable type inference failed */
    public static <T> T get(Context context, String str, T t) {
        SharedPreferences sharedPreferences = context.getSharedPreferences(FILE_NAME, 0);
        if (t instanceof String) {
            return (T) sharedPreferences.getString(str, (String) t);
        }
        if (t instanceof Integer) {
            return (T) Integer.valueOf(sharedPreferences.getInt(str, ((Integer) t).intValue()));
        }
        if (t instanceof Boolean) {
            return (T) Boolean.valueOf(sharedPreferences.getBoolean(str, ((Boolean) t).booleanValue()));
        }
        if (t instanceof Float) {
            return (T) Float.valueOf(sharedPreferences.getFloat(str, ((Float) t).floatValue()));
        }
        if (t instanceof Long) {
            return (T) Long.valueOf(sharedPreferences.getLong(str, ((Long) t).longValue()));
        }
        return (T) sharedPreferences.getString(str, (String) t);
    }

    public static void remove(Context context, String str) {
        SharedPreferences.Editor editorEdit = context.getSharedPreferences(FILE_NAME, 0).edit();
        editorEdit.remove(str);
        SharedPreferencesCompat.apply(editorEdit);
    }

    public static void clear(Context context) {
        SharedPreferences.Editor editorEdit = context.getSharedPreferences(FILE_NAME, 0).edit();
        editorEdit.clear();
        SharedPreferencesCompat.apply(editorEdit);
    }

    public static boolean contains(Context context, String str) {
        return context.getSharedPreferences(FILE_NAME, 0).contains(str);
    }

    public static Map<String, ?> getAll(Context context) {
        return context.getSharedPreferences(FILE_NAME, 0).getAll();
    }

    private static class SharedPreferencesCompat {
        private static final Method sApplyMethod = findApplyMethod();

        private SharedPreferencesCompat() {
        }

        private static Method findApplyMethod() {
            try {
                return SharedPreferences.Editor.class.getMethod("apply", new Class[0]);
            } catch (NoSuchMethodException e) {
                e.printStackTrace();
                return null;
            }
        }

        public static void apply(SharedPreferences.Editor editor) {
            try {
                Method method = sApplyMethod;
                if (method != null) {
                    method.invoke(editor, new Object[0]);
                    return;
                }
            } catch (Exception e) {
                e.printStackTrace();
            }
            editor.commit();
        }
    }
}