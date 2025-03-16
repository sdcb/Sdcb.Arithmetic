#include <stdio.h>
#include <gmp.h>

int main() {
    mpf_t a;
    mpf_init(a);         // 初始化高精度浮点数
    mpf_set_si(a, -99);  // 设置为 -99

    double d = mpf_get_d(a);
    long si = mpf_get_si(a);

    printf("mpf_get_d:  %f\n", d);
    printf("mpf_get_si: %ld\n", si);

    // print sizeof result of mpf_get_d and mpf_get_si
	printf("sizeof(mpf_get_d):  %lu\n", mpf_get_d(a));
	printf("sizeof(mpf_get_si): %lu\n", mpf_get_si(a));

    mpf_clear(a);        // 释放 mpf_t 占用的资源
    return 0;
}