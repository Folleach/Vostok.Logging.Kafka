namespace Folleach.Vostok.Logging.Kafka.Tests;

internal class Docker
{
    public static async Task<IAsyncDisposable> RunContainers(string composeFileName)
    {
        var dockerVersion = await Shell.RunCommand("docker", "--version");
        if (dockerVersion.ExitCode != 0)
            throw new InvalidOperationException("docker is required for tests");

        var composeUp = await Shell.RunCommand("docker", $"compose -f ./{composeFileName} up -d");
        if (composeUp.ExitCode != 0)
            throw new InvalidOperationException("docker containers cannot be started. Make sure that your user has a 'docker' group. Or add group: 'usermod -aG docker your_user_name'");
        return new DockerContainerRegistrar(composeFileName);
    }

    private class DockerContainerRegistrar : IAsyncDisposable
    {
        private string composeFileName;

        public DockerContainerRegistrar(string composeFileName)
        {
            this.composeFileName = composeFileName;
        }

        public async ValueTask DisposeAsync()
        {
            await Shell.RunCommand("docker", $"compose -f ./{composeFileName} down");
        }
    }
}
