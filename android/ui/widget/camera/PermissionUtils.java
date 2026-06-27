package cn.com.heaton.shiningmask.ui.widget.camera;

import android.content.Context;
import com.yanzhenjie.permission.Action;
import com.yanzhenjie.permission.AndPermission;
import com.yanzhenjie.permission.Rationale;
import com.yanzhenjie.permission.RequestExecutor;

/* JADX INFO: loaded from: classes.dex */
public class PermissionUtils {

    public interface PermissionListener {
        void onFailed(Context context);

        void onSuccess(Context context);
    }

    public static void applicationPermissions(final Context context, final PermissionListener permissionListener, String[]... strArr) {
        if (!AndPermission.hasPermissions(context, strArr)) {
            AndPermission.with(context).runtime().permission(strArr).rationale(new Rationale() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda0
                @Override // com.yanzhenjie.permission.Rationale
                public final void showRationale(Context context2, Object obj, RequestExecutor requestExecutor) {
                    requestExecutor.execute();
                }
            }).onGranted(new Action() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda1
                @Override // com.yanzhenjie.permission.Action
                public final void onAction(Object obj) {
                    permissionListener.onSuccess(context);
                }
            }).onDenied(new Action() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda2
                @Override // com.yanzhenjie.permission.Action
                public final void onAction(Object obj) {
                    permissionListener.onFailed(context);
                }
            }).start();
        } else {
            permissionListener.onSuccess(context);
        }
    }

    public static void applicationPermissions(final Context context, final PermissionListener permissionListener, String... strArr) {
        if (!AndPermission.hasPermissions(context, strArr)) {
            AndPermission.with(context).runtime().permission(strArr).rationale(new Rationale() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda3
                @Override // com.yanzhenjie.permission.Rationale
                public final void showRationale(Context context2, Object obj, RequestExecutor requestExecutor) {
                    requestExecutor.execute();
                }
            }).onGranted(new Action() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda4
                @Override // com.yanzhenjie.permission.Action
                public final void onAction(Object obj) {
                    permissionListener.onSuccess(context);
                }
            }).onDenied(new Action() { // from class: cn.com.heaton.shiningmask.ui.widget.camera.PermissionUtils$$ExternalSyntheticLambda5
                @Override // com.yanzhenjie.permission.Action
                public final void onAction(Object obj) {
                    permissionListener.onFailed(context);
                }
            }).start();
        } else {
            permissionListener.onSuccess(context);
        }
    }
}