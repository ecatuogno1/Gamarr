import PropTypes from 'prop-types';
import React from 'react';
import DescriptionList from 'Components/DescriptionList/DescriptionList';
import DescriptionListItem from 'Components/DescriptionList/DescriptionListItem';
import Button from 'Components/Link/Button';
import Modal from 'Components/Modal/Modal';
import ModalBody from 'Components/Modal/ModalBody';
import ModalContent from 'Components/Modal/ModalContent';
import ModalFooter from 'Components/Modal/ModalFooter';
import ModalHeader from 'Components/Modal/ModalHeader';
import { sizes } from 'Helpers/Props';
import translate from 'Utilities/String/translate';
import { formatVersion, getRegionNames, getReleaseTypeName } from './releaseDetails';

function FileDetailsModal(props) {
  const {
    isOpen,
    onModalClose,
    relativePath,
    edition,
    regions,
    version,
    releaseType,
    discNumber,
    discCount,
    companionFiles
  } = props;

  return (
    <Modal
      isOpen={isOpen}
      onModalClose={onModalClose}
      size={sizes.SMALL}
    >
      <ModalContent
        onModalClose={onModalClose}
      >
        <ModalHeader>
          {translate('Details')}
        </ModalHeader>

        <ModalBody>
          <DescriptionList>
            <DescriptionListItem
              title={translate('RelativePath')}
              data={relativePath}
            />

            {
              edition ?
                <DescriptionListItem title={translate('Edition')} data={edition} /> :
                null
            }

            {
              regions && regions.length ?
                <DescriptionListItem title={translate('CustomFormatsSpecificationRegion')} data={getRegionNames(regions)} /> :
                null
            }

            {
              version ?
                <DescriptionListItem title={translate('Version')} data={formatVersion(version)} /> :
                null
            }

            {
              releaseType ?
                <DescriptionListItem title={translate('CustomFormatsSpecificationReleaseType')} data={getReleaseTypeName(releaseType)} /> :
                null
            }

            {
              discNumber ?
                <DescriptionListItem title="Disc" data={discCount ? `${discNumber} / ${discCount}` : discNumber} /> :
                null
            }

            {
              companionFiles && companionFiles.length ?
                <DescriptionListItem title={translate('Files')} data={companionFiles.length + 1} /> :
                null
            }
          </DescriptionList>
        </ModalBody>

        <ModalFooter>
          <Button onPress={onModalClose}>
            {translate('Close')}
          </Button>
        </ModalFooter>
      </ModalContent>
    </Modal>
  );
}

FileDetailsModal.propTypes = {
  isOpen: PropTypes.bool.isRequired,
  onModalClose: PropTypes.func.isRequired,
  relativePath: PropTypes.string,
  edition: PropTypes.string,
  regions: PropTypes.arrayOf(PropTypes.string),
  version: PropTypes.string,
  releaseType: PropTypes.string,
  discNumber: PropTypes.number,
  discCount: PropTypes.number,
  companionFiles: PropTypes.arrayOf(PropTypes.string)
};

export default FileDetailsModal;
