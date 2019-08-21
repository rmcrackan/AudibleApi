// https://ricky.lalwani.me/blog/logging-in-to-amazon-part-2

function update(x) {
    var map = [];
    if (map.length == 0) {
        var poly = 3988292384;
        var i = 0;
        for (; i < 256; i++) {
            var temp = i;
            var at = 0;
            for (; at < 8; at++) {
                if (temp & 1 == 1) {
                    temp = temp >>> 1 ^ poly;
                } else {
                    temp = temp >>> 1;
                }
            }
            map[i] = temp;
        }
    }
    var crc = 0;
    var objUid;
    crc = crc ^ 4294967295;
    i = 0;
    for (; i < x.length; i++) {
        objUid = (crc ^ x.charCodeAt(i)) & 255;
        crc = crc >>> 8 ^ map[objUid];
    }
    crc = crc ^ 4294967295;
    return crc;
}

function format(dec) {
    var hexDigits = "0123456789ABCDEF";
    return [
        hexDigits.charAt(dec >>> 28 & 15),
        hexDigits.charAt(dec >>> 24 & 15),
        hexDigits.charAt(dec >>> 20 & 15),
        hexDigits.charAt(dec >>> 16 & 15),
        hexDigits.charAt(dec >>> 12 & 15),
        hexDigits.charAt(dec >>> 8 & 15),
        hexDigits.charAt(dec >>> 4 & 15),
        hexDigits.charAt(dec & 15)
    ].join("");
}

function parse(value) {
    if (value.length == 0) {
        return "";
    }
    var result = [4169969034, 4087877101, 1706678977, 3681020276];
    var n = Math.ceil(value.length / 4);
    var a = [];
    var i = 0;
    for (; i < n; i++) {
        a[i] = (value.charCodeAt(i * 4) & 255) + ((value.charCodeAt(i * 4 + 1) & 255) << 8) + ((value.charCodeAt(i * 4 + 2) & 255) << 16) + ((value.charCodeAt(i * 4 + 3) & 255) << 24);
    }
    var chunk = 2654435769;
    var aC = Math.floor(6 + 52 / n);
    var a12 = a[0];
    var next = a[n - 1];
    var d = 0;
    for (; aC-- > 0;) {
        d += chunk;
        var aw = d >>> 2 & 3;
        var j = 0;
        for (; j < n; j++) {
            a12 = a[(j + 1) % n];
            next = a[j] += (next >>> 5 ^ a12 << 2) + (a12 >>> 3 ^ next << 4) ^ (d ^ a12) + (result[j & 3 ^ aw] ^ next);
        }
    }
    var tmp_arr = [];
    i = 0;
    for (; i < n; i++) {
        tmp_arr[i] = String.fromCharCode(a[i] & 255, a[i] >>> 8 & 255, a[i] >>> 16 & 255, a[i] >>> 24 & 255);
    }
    return tmp_arr.join("");
}

function evaluate(input) {
    var ret = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=";
    var UNICODE_SPACES = [];
    var av;
    var val2;
    var chr2;
    var next;
    var j;
    var idx;
    var i;
    var index = 0;
    for (; index < input.length;) {
        av = input.charCodeAt(index++);
        val2 = input.charCodeAt(index++);
        chr2 = input.charCodeAt(index++);
        next = av >> 2;
        j = (av & 3) << 4 | val2 >> 4;
        idx = (val2 & 15) << 2 | chr2 >> 6;
        i = chr2 & 63;
        if (isNaN(val2)) {
            idx = i = 64;
        }
        else {
            if (isNaN(chr2)) {
                i = 64;
            }
        }
        UNICODE_SPACES.push(ret.charAt(next));
        UNICODE_SPACES.push(ret.charAt(j));
        UNICODE_SPACES.push(ret.charAt(idx));
        UNICODE_SPACES.push(ret.charAt(i));
    }
    return UNICODE_SPACES.join("");
}
