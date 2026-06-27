package cn.com.heaton.shiningmask.base;

import cn.com.heaton.shiningmask.base.IBaseView;

/* JADX INFO: loaded from: classes.dex */
public interface IBasePresenter<V extends IBaseView> {
    void attachView(V v);
}