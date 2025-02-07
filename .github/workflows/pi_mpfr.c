#include <stdio.h>
#include <mpfr.h>

int main() {
	mpfr_t pi;
	mpfr_init2(pi, 256);

	mpfr_const_pi(pi, MPFR_RNDN);

	printf("Pi with 256-bit precision:\n");
	mpfr_out_str(stdout, 10, 0, pi, MPFR_RNDN);
	printf("\n");

	mpfr_clear(pi);

	return 0;
}