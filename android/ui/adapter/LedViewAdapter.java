package cn.com.heaton.shiningmask.ui.adapter;

import android.content.Context;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.model.bean.TextData;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter;
import cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerViewHolder;
import cn.com.heaton.shiningmask.ui.utils.LogUtil;
import cn.com.heaton.shiningmask.ui.widget.ledaddview.LedAddView;
import java.util.List;

/* JADX INFO: loaded from: classes.dex */
public class LedViewAdapter extends RecyclerAdapter<TextData> {
    private Context context;

    @Override // cn.com.heaton.shiningmask.ui.adapter.baseadapter.RecyclerAdapter
    public void convert(RecyclerViewHolder recyclerViewHolder, TextData textData) {
        LedAddView ledAddView = (LedAddView) recyclerViewHolder.getView(R.id.ltv_preview);
        ledAddView.setMode(1);
        ledAddView.setPointMargin(0);
        ledAddView.setLayerType(1, null);
        LogUtil.d("textData:" + textData.getWidthCount() + " textData.getWidthCount:" + textData.getWidthCount());
        ledAddView.init(textData.getWidthCount(), 16);
        LogUtil.d("textColor:" + textData.getColor());
        if (textData.getColor() != 0) {
            ledAddView.setSelectedColor(textData.getColor());
        }
        ledAddView.setTextData(textData.getData());
    }

    public LedViewAdapter(Context context, int i, List<TextData> list) {
        super(context, i, list);
        this.context = context;
    }
}