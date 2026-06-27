package cn.com.heaton.shiningmask.contract;

import cn.com.heaton.shiningmask.base.IBasePresenter;
import cn.com.heaton.shiningmask.base.IBaseView;

/* JADX INFO: loaded from: classes.dex */
public interface MainContract {

    public interface Presenter extends IBasePresenter<View> {
        void testDb();

        void testGetMpresenter();

        void testPreference();

        void testRequestNetwork();
    }

    public interface View extends IBaseView {
        void testGetMview();
    }
}