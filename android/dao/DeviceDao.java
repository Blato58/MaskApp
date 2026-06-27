package cn.com.heaton.shiningmask.dao;

import android.database.Cursor;
import android.database.sqlite.SQLiteStatement;
import cn.com.heaton.shiningmask.dao.bean.Device;
import org.greenrobot.greendao.AbstractDao;
import org.greenrobot.greendao.Property;
import org.greenrobot.greendao.database.Database;
import org.greenrobot.greendao.database.DatabaseStatement;
import org.greenrobot.greendao.internal.DaoConfig;

/* JADX INFO: loaded from: classes.dex */
public class DeviceDao extends AbstractDao<Device, Long> {
    public static final String TABLENAME = "DEVICE";

    public static class Properties {
        public static final Property Id = new Property(0, Long.class, "id", true, "_id");
        public static final Property DeviceName = new Property(1, String.class, "deviceName", false, "DEVICE_NAME");
        public static final Property Mac = new Property(2, String.class, "mac", false, "MAC");
        public static final Property IsReConnect = new Property(3, Boolean.TYPE, "isReConnect", false, "IS_RE_CONNECT");
    }

    @Override // org.greenrobot.greendao.AbstractDao
    protected final boolean isEntityUpdateable() {
        return true;
    }

    public DeviceDao(DaoConfig daoConfig) {
        super(daoConfig);
    }

    public DeviceDao(DaoConfig daoConfig, DaoSession daoSession) {
        super(daoConfig, daoSession);
    }

    public static void createTable(Database database, boolean z) {
        database.execSQL("CREATE TABLE " + (z ? "IF NOT EXISTS " : "") + "\"DEVICE\" (\"_id\" INTEGER PRIMARY KEY AUTOINCREMENT ,\"DEVICE_NAME\" TEXT,\"MAC\" TEXT,\"IS_RE_CONNECT\" INTEGER NOT NULL );");
    }

    public static void dropTable(Database database, boolean z) {
        database.execSQL("DROP TABLE " + (z ? "IF EXISTS " : "") + "\"DEVICE\"");
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(DatabaseStatement databaseStatement, Device device) {
        databaseStatement.clearBindings();
        Long id = device.getId();
        if (id != null) {
            databaseStatement.bindLong(1, id.longValue());
        }
        String deviceName = device.getDeviceName();
        if (deviceName != null) {
            databaseStatement.bindString(2, deviceName);
        }
        String mac = device.getMac();
        if (mac != null) {
            databaseStatement.bindString(3, mac);
        }
        databaseStatement.bindLong(4, device.getIsReConnect() ? 1L : 0L);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final void bindValues(SQLiteStatement sQLiteStatement, Device device) {
        sQLiteStatement.clearBindings();
        Long id = device.getId();
        if (id != null) {
            sQLiteStatement.bindLong(1, id.longValue());
        }
        String deviceName = device.getDeviceName();
        if (deviceName != null) {
            sQLiteStatement.bindString(2, deviceName);
        }
        String mac = device.getMac();
        if (mac != null) {
            sQLiteStatement.bindString(3, mac);
        }
        sQLiteStatement.bindLong(4, device.getIsReConnect() ? 1L : 0L);
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public Long readKey(Cursor cursor, int i) {
        if (cursor.isNull(i)) {
            return null;
        }
        return Long.valueOf(cursor.getLong(i));
    }

    /* JADX WARN: Can't rename method to resolve collision */
    @Override // org.greenrobot.greendao.AbstractDao
    public Device readEntity(Cursor cursor, int i) {
        int i2 = i + 1;
        int i3 = i + 2;
        return new Device(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)), cursor.isNull(i2) ? null : cursor.getString(i2), cursor.isNull(i3) ? null : cursor.getString(i3), cursor.getShort(i + 3) != 0);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public void readEntity(Cursor cursor, Device device, int i) {
        device.setId(cursor.isNull(i) ? null : Long.valueOf(cursor.getLong(i)));
        int i2 = i + 1;
        device.setDeviceName(cursor.isNull(i2) ? null : cursor.getString(i2));
        int i3 = i + 2;
        device.setMac(cursor.isNull(i3) ? null : cursor.getString(i3));
        device.setIsReConnect(cursor.getShort(i + 3) != 0);
    }

    /* JADX INFO: Access modifiers changed from: protected */
    @Override // org.greenrobot.greendao.AbstractDao
    public final Long updateKeyAfterInsert(Device device, long j) {
        device.setId(Long.valueOf(j));
        return Long.valueOf(j);
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public Long getKey(Device device) {
        if (device != null) {
            return device.getId();
        }
        return null;
    }

    @Override // org.greenrobot.greendao.AbstractDao
    public boolean hasKey(Device device) {
        return device.getId() != null;
    }
}