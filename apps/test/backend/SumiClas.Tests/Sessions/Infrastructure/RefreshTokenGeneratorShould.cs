namespace SumiClas.Tests.Sessions.Infrastructure;

using AwesomeAssertions;
using SumiClas.Sessions.Infrastructure;

[TestFixture]
public class RefreshTokenGeneratorShould
{
    private readonly RefreshTokenGenerator _generator = new();

    [Test]
    public void return_a_43_char_base64url_value_without_padding_when_generating()
    {
        string raw = this._generator.Generate();

        raw.Should().HaveLength(43);
        raw.Should().MatchRegex("^[A-Za-z0-9_-]+$");
    }

    [Test]
    public void return_different_values_when_generating_twice()
    {
        string first = this._generator.Generate();
        string second = this._generator.Generate();

        first.Should().NotBe(second);
        this._generator.Hash(first).Should().NotBe(this._generator.Hash(second));
    }

    [Test]
    public void return_a_64_char_lowercase_hex_value_that_is_not_the_raw_value_when_hashing()
    {
        string raw = this._generator.Generate();

        string hash = this._generator.Hash(raw);

        hash.Should().HaveLength(64);
        hash.Should().MatchRegex("^[0-9a-f]{64}$");
        hash.Should().NotBe(raw);
    }

    [Test]
    public void return_the_same_hash_when_hashing_the_same_value()
    {
        string raw = this._generator.Generate();

        this._generator.Hash(raw).Should().Be(this._generator.Hash(raw));
    }

    [Test]
    public void match_the_known_sha256_of_the_utf8_bytes_when_hashing()
    {
        this._generator.Hash("abc")
            .Should().Be("ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad");
    }
}
