package cn.com.heaton.shiningmask.ui.utils;

import android.util.Log;
import java.io.IOException;
import okhttp3.FormBody;
import okhttp3.Interceptor;
import okhttp3.Request;
import okhttp3.Response;
import org.apache.http.client.methods.HttpPost;

/* JADX INFO: loaded from: classes.dex */
public class LogInterceptor implements Interceptor {
    @Override // okhttp3.Interceptor
    public Response intercept(Interceptor.Chain chain) throws IOException {
        Request request = chain.request();
        long jNanoTime = System.nanoTime();
        String strMethod = request.method();
        StringBuilder sb = new StringBuilder();
        if (HttpPost.METHOD_NAME.equals(strMethod) && (request.body() instanceof FormBody)) {
            FormBody formBody = (FormBody) request.body();
            for (int i = 0; i < formBody.size(); i++) {
                sb.append(formBody.encodedName(i) + "=" + formBody.encodedValue(i) + ",");
            }
            sb.delete(sb.length() - 1, sb.length());
        }
        Log.i("LogInterceptor", String.format("发送请求 %s on %s%n%s%s", request.url(), chain.connection(), request.headers(), "RequestParams:{" + sb.toString() + "}"));
        Response responseProceed = chain.proceed(request);
        long jNanoTime2 = System.nanoTime();
        Log.i("LogInterceptor", String.format("接收响应: [%s] %n返回json:【%s】 %.1fms%n%s", responseProceed.request().url(), responseProceed.peekBody(1048576L).string(), Double.valueOf((jNanoTime2 - jNanoTime) / 1000000.0d), responseProceed.headers()));
        return responseProceed;
    }
}