namespace DataMigratorTest.Validation.FileContainer.Base
{
    using System.Collections.Generic;
    using System.IO;
    using DataMigrator.Container.Base.Header;
    using DataMigrator.Container.FileContainer;
    using FluentAssertions;

    public abstract class PartedFileContainerValidatorBase<TContainer, TFileHeader> :
        FileContainerValidatorBase<TContainer, TFileHeader>
        where TContainer : FileContainerInfoBase<TContainer, TFileHeader>
		where TFileHeader : class, IFilesystemHeader<FileInfo>
    {
        private IList<TContainer> _relatedParts;

        public override void ValidateContentHeader(TContainer container, FileInfo sourceInfo)
        {
            base.ValidateContentHeader(container, sourceInfo);
            foreach (var partialContainer in GetRelatedParts(container))
            {
                partialContainer.ContentHeader.Should().NotBeNull();
            }
        }

        public override void ValidateMigrationContainer(TContainer container, FileInfo sourceInfo)
        {
            base.ValidateMigrationContainer(container, sourceInfo);
            foreach (var partialContainer in GetRelatedParts(container))
            {
                base.ValidateMigrationContainer(partialContainer, sourceInfo);
            }
        }

        public override void ValidateStartHeader(TContainer container, FileInfo sourceInfo)
        {
            base.ValidateStartHeader(container, sourceInfo);
            container.StartHeader.PartNumber.Should().Be(0);
            container.StartHeader.NextHeaderLength.Should().NotBe(0);
            container.StartHeader.IsLastHeader().Should().BeFalse();

            container.StartHeader.Parts.Should().Be(GetRelatedParts(container).Count + 1); // add the excluded main part
            foreach (var partialContainer in GetRelatedParts(container))
            {
                base.ValidateStartHeader(partialContainer, sourceInfo);

                var parsed = int.Parse(partialContainer.FileInfo.Extension.Replace(".part", string.Empty));

                partialContainer.StartHeader.PartNumber.Should().Be(parsed);
                partialContainer.StartHeader.Parts.Should().Be(container.StartHeader.Parts);
                partialContainer.StartHeader.ContainerId.Should().Be(container.StartHeader.ContainerId);
                partialContainer.StartHeader.NextHeaderLength.Should().Be(0);
                partialContainer.StartHeader.IsLastHeader().Should().BeTrue();
            }
        }

        protected IList<TContainer> GetRelatedParts(TContainer container)
        {
            return _relatedParts ?? (_relatedParts = container.FindRelatedParts());
        }
    }
}