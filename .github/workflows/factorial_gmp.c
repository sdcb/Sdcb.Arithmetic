#include <stdio.h>
#include <gmp.h>
#include <time.h>

void calculate_factorial(unsigned long n, mpz_t result) {
    mpz_set_ui(result, 1);
    for (unsigned long i = 1; i <= n; ++i) {
        mpz_mul_ui(result, result, i);
    }
}

int main() {
    unsigned long n = 100000;
    mpz_t result;
    mpf_t fresult;
    mpz_init(result);

    clock_t start, end;
    double cpu_time_used;

    for (int i = 0; i < 5; i++) {
        start = clock();
        calculate_factorial(n, result);
        end = clock();
        cpu_time_used = ((double)(end - start)) / CLOCKS_PER_SEC;
        printf("Round %d time taken to calculate %lu! is %f seconds.\n", i, n, cpu_time_used);
    }

    mpf_init2(fresult, 1000000);
    mpf_set_z(fresult, result);
    mpf_printf("The factorial of %lu is %.20Fe\n", n, fresult);

    mpf_clear(fresult);
    mpz_clear(result);
    return 0;
}