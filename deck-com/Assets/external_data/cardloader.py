#run using “python3” not “python”
#You need to use the version you installed (python3) not the system version (python)

from __future__ import print_function
import httplib2
import os

from apiclient import discovery
from oauth2client import client
from oauth2client import tools
from oauth2client.file import Storage

import sys

try:
    import argparse
    flags = argparse.ArgumentParser(parents=[tools.argparser]).parse_args()
except ImportError:
    flags = None

# If modifying these scopes, delete your previously saved credentials
# at ~/.credentials/sheets.googleapis.com-python-quickstart.json
SCOPES = 'https://www.googleapis.com/auth/spreadsheets.readonly'
CLIENT_SECRET_FILE = 'client_secret.json'
APPLICATION_NAME = 'Google Sheets API Python Quickstart'

#google's code for grabbing the credentials
def get_credentials():
    home_dir = os.path.expanduser('~')
    credential_dir = os.path.join(home_dir, '.credentials')
    if not os.path.exists(credential_dir):
        os.makedirs(credential_dir)
    credential_path = os.path.join(credential_dir,
                                   'sheets.googleapis.com-python-quickstart.json')

    store = Storage(credential_path)
    credentials = store.get()
    if not credentials or credentials.invalid:
        flow = client.flow_from_clientsecrets(CLIENT_SECRET_FILE, SCOPES)
        flow.user_agent = APPLICATION_NAME
        if flags:
            credentials = tools.run_flow(flow, store, flags)
        else: # Needed only for compatibility with Python 2.6
            credentials = tools.run(flow, store)
        print('Storing credentials to ' + credential_path)
    return credentials

def main():
    credentials = get_credentials()
    http = credentials.authorize(httplib2.Http())
    discoveryUrl = ('https://sheets.googleapis.com/$discovery/rest?'
                    'version=v4')
    service = discovery.build('sheets', 'v4', http=http,
                              discoveryServiceUrl=discoveryUrl)

    #id part of my spreadhseet url
    spreadsheetId = '1X2tsHz0IEwAQ4X9_Z7RRYn7qM3_m8j6yDkQu3e-c7WE'
    rangeName = 'Sheet1!A2:P'
    result = service.spreadsheets().values().get(
        spreadsheetId=spreadsheetId, range=rangeName).execute()
    values = result.get('values', [])

    if not values:
        print('No data found.')
    else:
        #we found stuff!
        file = open('cards.xml', 'w')
        file.write('<cards>\n')
        for row in values:
            #get all of the info
            displayName = row[0]
            idName = row[1]
            level = row[2]
            if level == 'n/a':
                level = -1
            scriptName = row[3]
            description = row[4]

            #remaining elements are additional values that the card might need
            otherVals = row[5:len(row)]
            

            file.write('<card idName="'+idName+'">\n')

            file.write('<name>'+displayName+'</name>\n')
            file.write('<level>'+str(level)+'</level>\n')
            file.write('<script>'+scriptName+'</script>\n')
            if description != ".":
                file.write('<desc>'+description+'</desc>\n')

            for val in otherVals:
                thisVal = val.split(':')
                file.write('<'+thisVal[0]+'>'+thisVal[1]+'</'+thisVal[0]+'>\n')

            file.write('</card>\n')

            # Print columns A and E, which correspond to indices 0 and 4.
            #print('%s, %s' % (row[0], row[4]))

        file.write('</cards>')
        file.close()


if __name__ == '__main__':
    main()