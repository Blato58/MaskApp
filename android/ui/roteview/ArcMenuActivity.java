package cn.com.heaton.shiningmask.ui.roteview;

import android.app.Activity;
import android.os.Bundle;
import android.view.View;
import android.widget.TextView;
import android.widget.Toast;
import cn.com.heaton.shiningmask.R;
import cn.com.heaton.shiningmask.ui.roteview.ArcMenu;

/* JADX INFO: loaded from: classes.dex */
public class ArcMenuActivity extends Activity {
    private ArcMenu arcMenu;

    @Override // android.app.Activity
    public void onCreate(Bundle bundle) {
        super.onCreate(bundle);
        setContentView(R.layout.activity_arcmenu);
        ArcMenu arcMenu = (ArcMenu) findViewById(R.id.id_arcmenu);
        this.arcMenu = arcMenu;
        arcMenu.setOnMenuItemClickListener(new ArcMenu.OnMenuItemClickListener() { // from class: cn.com.heaton.shiningmask.ui.roteview.ArcMenuActivity.1
            @Override // cn.com.heaton.shiningmask.ui.roteview.ArcMenu.OnMenuItemClickListener
            public void onClick(View view, int i) {
                Toast.makeText(ArcMenuActivity.this, ((TextView) view).getText().toString(), 0).show();
            }
        });
    }
}