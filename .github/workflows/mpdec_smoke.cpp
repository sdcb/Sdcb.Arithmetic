#include <iostream>
#include <string>
#include <decimal.hh>

using decimal::Context;
using decimal::Decimal;

int main()
{
    decimal::context.prec(50);

    const std::string actual = (Decimal(1) / Decimal(7)).to_sci();
    const std::string expected = "0.14285714285714285714285714285714285714285714285714";
    if (actual != expected) {
        std::cerr << "FAIL 1/7 (prec=50): expected " << expected << ", got " << actual << std::endl;
        return 1;
    }
    std::cout << "OK   1/7 (prec=50) = " << actual << std::endl;

    const std::string sum = (Decimal("0.1") + Decimal("0.2")).to_sci();
    if (sum != "0.3") {
        std::cerr << "FAIL 0.1+0.2: expected 0.3, got " << sum << std::endl;
        return 1;
    }
    std::cout << "OK   0.1+0.2 = " << sum << std::endl;

    std::cout << "libmpdec++ smoke test passed" << std::endl;
    return 0;
}
