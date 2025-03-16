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

    mpf_clear(a);        // 释放 mpf_t 占用的资源
    return 0;
}